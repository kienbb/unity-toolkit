using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace HorusFW.DI
{
    public class HorusDI
    {
        private static readonly Dictionary<object, HorusDIContainer> Containers = new Dictionary<object, HorusDIContainer>();
        private static readonly List<object> InvalidSourceObjects = new List<object>();
        private static readonly List<object> InvalidPresenterObjects = new List<object>();
        public static object globalContainerKey { get; private set; } = new object();
        private static HorusDIContainer GetContainer(object containerKey)
        {
            if (containerKey == null)
                containerKey = globalContainerKey;
            if (Containers.TryGetValue(containerKey, out HorusDIContainer container) == false)
            {
                container = new HorusDIContainer();
                Containers.Add(containerKey, container);
            }
            return container;
        }

        /// <summary>
        /// group các source lại thành 1
        /// </summary>
        /// <param name="dependencySources"></param>
        /// <returns></returns>
        public static HorusDIInstallerComposition Compose(object containerKey, object[] dependencySources)
        {
            if (dependencySources.Length == 0)
                return null;
            var sources = new HorusDIInstallerComposition();
            sources.AddRange(dependencySources);
            for (int i = 0; i < sources.Count; i++)
            {
                Register(sources[i], containerKey);
            }
            return sources;
        }

        /// <summary>
        /// register dependency vào container tổng, nếu object này đã được đăng kí rồi thì bỏ qua
        /// </summary>
        /// <param name="source"></param>
        public static (object, object) Register(object source, object containerKey)
        {
            if (source == null)
                return (source, containerKey);
            var container = GetContainer(containerKey);

            if (container.classDependencies.Exists(x => x.Item2 == source)
                || container.methodDependencies.Exists(x => x.Item2 == source)
                || container.fieldDependencies.Exists(x => x.Item2 == source)
                || container.propertyDependencies.Exists(x => x.Item2 == source))
                return (source, containerKey);

            Type dType = source.GetType();
            if (source.GetType().IsClass == false)
                return (source, containerKey);
            ///check instance xem có phải được register không
            ///
            if (System.Attribute.IsDefined(dType, typeof(ClassInjectionAttribute)))
            {
                CleanClass(container);
                if (container.classDependencies.Contains((dType, source)) == false)
                {
                    container.classDependencies.Add((dType, source));
                }
            }

            MethodInfo[] dMethods = dType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            bool needClean = false;
            if (dMethods.Length > 0)
            {
                foreach (MethodInfo method in dMethods)
                {
                    if (System.Attribute.IsDefined(method, typeof(MethodInjectionAttribute)))
                    {
                        needClean = true;
                        var dMethod = (dType, source, method);
                        if (container.methodDependencies.Contains(dMethod) == false)
                        {
                            container.methodDependencies.Add(dMethod);
                        }
                    }
                }
            }
            if (needClean)
                CleanMethod(container);

            needClean = false;
            FieldInfo[] dFields = dType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (dFields.Length > 0)
            {
                foreach (FieldInfo field in dFields)
                {
                    if (System.Attribute.IsDefined(field, typeof(FieldInjectionAttribute)))
                    {
                        needClean = true;
                        var dField = (dType, source, field);
                        if (container.fieldDependencies.Contains(dField) == false)
                        {
                            container.fieldDependencies.Add(dField);
                        }
                    }
                }
            }
            if (needClean)
                CleanField(container);

            needClean = false;
            //clean injected
            var properties = dType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.IsDefined(typeof(FieldInjectionAttribute)));
            if (properties != null)
            {
                foreach (PropertyInfo property in properties)
                {
                    needClean = true;
                    var dField = (dType, source, property);
                    if (container.propertyDependencies.Contains(dField) == false)
                    {
                        container.propertyDependencies.Add(dField);
                    }
                }
            }
            if (needClean)
                CleanProperty(container);

            ///clean waiting inject
            ///
            if (container.waitingInject.Count > 0)
            {
                container.waitingInject.RemoveAll(x =>
                {
                    bool injectSuccess = TryInject(x.Item1, x.Item2, x.Item3, ref source, container, x.Item4, x.Item5);
                    if (injectSuccess)
                    {
                        container.AddInjectionPair((source, x.Item2, x.Item3));
                    }

                    return x.Item2 == null || injectSuccess;
                });
            }

            return (source, containerKey);
        }

        /// <summary>
        /// gọi khi object dispose hoặc bị destroy, 
        /// gỡ toàn bộ các phần được inject vào obj này cũng như các phần mà nó cung cấp ra bên ngoài
        /// </summary>
        /// <param name="obj"></param>
        public static void UnRegister(object obj, object containerKey)
        {
            if (obj != null)
            {
                var container = GetContainer(containerKey);
                //clean dependency distribution
                container.classDependencies.RemoveAll(x => x.Item2 == obj);
                container.methodDependencies.RemoveAll(x => x.Item2 == obj);
                container.fieldDependencies.RemoveAll(x => x.Item2 == obj);
                container.propertyDependencies.RemoveAll(x => x.Item2 == obj);

                //clean injected
                FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fields.Length > 0)
                {
                    foreach (FieldInfo field in fields)
                    {
                        var attributes = field.GetCustomAttributes<InjectAttribute>();
                        if (attributes != null && attributes.Count() > 0)
                        {
                            field.SetValue(obj, null);
                        }
                    }
                }

                //clean this waiting injection
                container.waitingInject.RemoveAll(x => x.Item2 == obj);

                //clean objects that using it
                var pairs = container.injectionPair.Where(x => x.Item1 == obj).ToList();
                if (pairs != null)
                {
                    for (int i = 0; i < pairs.Count; i++)
                    {
                        var pair = pairs.ElementAt(i);
                        pair.Item3.SetValue(pair.Item2, null);
                        container.injectionPair.Remove(pair);
                    }
                }
            }
            InvalidSourceObjects.RemoveAll(x => x == obj || x == null);
            InvalidPresenterObjects.RemoveAll(x => x == obj || x == null);
        }

        /// <summary>
        /// inject method, class cần thiết vào presenter từ các dependency đã đăng kí
        /// </summary>
        /// <param name="presenter"></param>
        public static (object, object) Inject(object source, object presenter, object containerKey, bool forceReinject = false)
        {
            if (presenter == null || presenter == source)
                return (presenter, containerKey);
            var container = GetContainer(containerKey);

            ///bỏ qua nếu đã inject cặp này rồi
            if (forceReinject == false && container.injectionPair.Exists(x => x.Item1 == source && x.Item2 == presenter))
                return (presenter, containerKey);

            Type pType = presenter.GetType();
            var _fields = pType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.IsDefined(typeof(InjectAttribute)));
            if (_fields != null)
            {
                var fields = _fields.ToArray();
                foreach (FieldInfo field in fields)
                {
                    var attributes = field.GetCustomAttributes<InjectAttribute>();
                    int attCount = attributes.Count();
                    {
                        for (int i = 0; i < attCount; i++)
                        {
                            var attribute = attributes.ElementAt(i);

                            bool injectSuccess = TryInject(attribute.SourceType, presenter, field, ref source, container, attribute.AcceptSubclass, attribute.SourceName);
                            if (injectSuccess)
                            {
                                container.AddInjectionPair((source, presenter, field));
                            }

                            if (injectSuccess == false && container.waitingInject.Contains((attribute.SourceType, presenter, field, attribute.AcceptSubclass, attribute.SourceName)) == false)
                            {
                                container.waitingInject.Add((attribute.SourceType,  presenter, field, attribute.AcceptSubclass, attribute.SourceName));
                            }
                        }
                    }
                }
            }
            return (presenter, containerKey);
        }

        private static bool TryInject(Type sourceType, object presenter, FieldInfo presenterField, ref object sourceObject, HorusDIContainer container, bool acceptSubclass, string sourceMemberName)
        {
            if (TryInjectClassInstance(presenter, presenterField, sourceType, ref sourceObject, container, acceptSubclass))
                return true;
            else if (TryInjectMethod(presenter, presenterField, sourceType, sourceMemberName, ref sourceObject, container, acceptSubclass))
                return true;
            else if (TryInjectField(presenter, presenterField, sourceType, sourceMemberName, ref sourceObject, container, acceptSubclass))
                return true;
            else return false;
        }

        ///nếu là inject instance
        private static bool TryInjectClassInstance(object presenter, FieldInfo presenterField, Type sourceType, ref object _sourceObject, HorusDIContainer container, bool acceptSubclass)
        {
            if (sourceType != null)
                return false;
            var sourceObject = _sourceObject;

            var source = container.classDependencies.FirstOrDefault(x => (x.Item2 == sourceObject && x.Item1 == presenterField.FieldType) || x.Item1 == presenterField.FieldType).Item2;
            if (acceptSubclass && source == null)
                source = container.classDependencies.FirstOrDefault(x => (x.Item2 == sourceObject && x.Item1.IsSubclassOf(presenterField.FieldType)) || x.Item1.IsSubclassOf(presenterField.FieldType)).Item2;
            if (source == null)
                return false;
            _sourceObject = source;
            presenterField.SetValue(presenter, source);
            return true;
        }

        ///nếu là inject method vào 1 action delegate
        private static bool TryInjectMethod(object presenter, FieldInfo presenterField, Type sourceType, string sourceMemberName, ref object _sourceObject, HorusDIContainer container, bool acceptSubclass)
        {
            IEnumerable<(Type, object, MethodInfo)> overloadMethodInfos = null;
            var sourceObject = _sourceObject;
            overloadMethodInfos = container.methodDependencies.Where(x => (x.Item2 == sourceObject && x.Item3.Name == sourceMemberName && x.Item1 == sourceType) || (x.Item3.Name == sourceMemberName && x.Item1 == sourceType));

            if (acceptSubclass && (overloadMethodInfos == null || overloadMethodInfos.Count() == 0))
            {
                overloadMethodInfos = container.methodDependencies.Where(x => (x.Item2 == sourceObject && x.Item3.Name == sourceMemberName && x.Item1.IsSubclassOf(sourceType)) || (x.Item3.Name == sourceMemberName && x.Item1.IsSubclassOf(sourceType)));
            }
            
            if (overloadMethodInfos != null && overloadMethodInfos.Count() > 0)
            {
                int overloadCount = overloadMethodInfos.Count();
                Exception e = null;
                bool success = false;
                for (int i = 0; i < overloadCount; i++)
                {
                    var mInfo = overloadMethodInfos.ElementAt(i);
                    ///nếu là inject method vào 1 action delegate, và action đó được declear body thì 
                    ///có thể check được inject lỗi ở phần nào, vd declear Func<T> GetT = () => new T;
                    ///nếu không có giá trị mặc định thì sẽ thử blind action 
                    ///
                    object fieldValue = presenterField.GetValue(presenter);
                    if (fieldValue is Delegate delegateInstance)
                    {
                        //Debug.Log($"{field.Name} is a delegate!");
                        if (delegateInstance.Method.ReturnType != mInfo.Item3.ReturnType)
                        {
                            Debug.LogError($"[Inject Error] {presenterField.DeclaringType.FullName}.{presenterField.Name} return type not match with {mInfo.Item3.DeclaringType.FullName}.{mInfo.Item3.Name}");
                            continue;
                        }
                        if (delegateInstance.GetMethodInfo().GetParameters().Select(x => x.ParameterType).SequenceEqual(mInfo.Item3.GetParameters().Select(x => x.ParameterType)) == false)
                        {
                            Debug.LogError($"[Inject Error] {presenterField.DeclaringType.FullName}.{presenterField.Name} argument types not match with {mInfo.Item3.DeclaringType.FullName}.{mInfo.Item3.Name}");
                            continue;
                        }
                    }
                    else
                    {
                        //Debug.LogWarning($"[Inject Warning] {presenterField.DeclaringType.FullName}.{presenterField.Name} does not have instantiated value, trying create blind action");
                    }
                    try
                    {
                        //var blindAction = Delegate.CreateDelegate(field.FieldType, mInfo.Item2, mInfo.Item3.Name);
                        var blindAction = mInfo.Item3.CreateDelegate(presenterField.FieldType, mInfo.Item2);
                        presenterField.SetValue(presenter, blindAction);
                        success = true;
                        _sourceObject = mInfo.Item2;
                        break;
                    }
                    catch (Exception ex)
                    {
                        e = ex;
                        success = false;
                        continue;
                    }
                }
                if (success == false && e != null)
                {
                    Debug.LogException(e);
                }
                return success;
            }
            ///check xem có field nào có thể inject đc vào ko
            ///
            var sourceProperty = container.propertyDependencies.FirstOrDefault(x => (x.Item2 == sourceObject && x.Item3.Name == sourceMemberName && x.Item1 == sourceType) || (x.Item3.Name == sourceMemberName && x.Item1 == sourceType));
            if (acceptSubclass && sourceProperty == default((Type, object, PropertyInfo)))
            {
                sourceProperty = container.propertyDependencies.FirstOrDefault(x => (x.Item2 == sourceObject && x.Item3.Name == sourceMemberName && x.Item1.IsSubclassOf(sourceType)) || (x.Item3.Name == sourceMemberName && x.Item1.IsSubclassOf(sourceType)));
            }
            if (sourceProperty != default((Type, object, PropertyInfo)))
            {
                var getter = sourceProperty.Item3.GetGetMethod() ?? sourceProperty.Item3.GetGetMethod(true);
                if (getter == null)
                    return false;
                try
                {
                    _sourceObject = sourceProperty.Item2;
                    var action = getter.CreateDelegate(presenterField.FieldType, sourceProperty.Item2);
                    presenterField.SetValue(presenter, action);
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return false;
                }
            }
            else return false;
        }

        ///nếu là inject 1 field của class này vào 1 field của class khác
        private static bool TryInjectField(object presenter, FieldInfo presenterField, Type sourceType, string sourceMemberName, ref object _sourceObject, HorusDIContainer container, bool acceptSubclass)
        {
            var sourceObject = _sourceObject;
            var sourceField = default((Type, object, FieldInfo));
            sourceField = container.fieldDependencies.FirstOrDefault(x => (x.Item2 == sourceObject && x.Item3.Name == sourceMemberName && x.Item1 == sourceType) || (x.Item3.Name == sourceMemberName && x.Item1 == sourceType));
            if(acceptSubclass && sourceField == default((Type, object, FieldInfo)))
                sourceField = container.fieldDependencies.FirstOrDefault(x => (x.Item2 == sourceObject && x.Item3.Name == sourceMemberName && x.Item1.IsSubclassOf(sourceType)) || (x.Item3.Name == sourceMemberName && x.Item1.IsSubclassOf(sourceType)));
            if (sourceField == default((Type, object, FieldInfo)))
                return false;
            try
            {
                _sourceObject = sourceField;
                presenterField.SetValue(presenter, sourceField.Item3.GetValue(sourceField));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        private static void CleanClass(HorusDIContainer container)
        {
            container.classDependencies.RemoveAll(x => x.Item2 == null);
        }
        private static void CleanMethod(HorusDIContainer container)
        {
            var kvps = container.methodDependencies.Where(x => x.Item2 == null);
            if (kvps != null)
                for (int i = 0; i < kvps.Count(); i++)
                {
                    container.methodDependencies.Remove(kvps.ElementAt(i));
                }
        }
        private static void CleanField(HorusDIContainer container)
        {
            var kvps = container.fieldDependencies.Where(x => x.Item2 == null);
            if (kvps != null)
                for (int i = 0; i < kvps.Count(); i++)
                {
                    container.fieldDependencies.Remove(kvps.ElementAt(i));
                }
        }
        private static void CleanProperty(HorusDIContainer container)
        {
            var kvps = container.propertyDependencies.Where(x => x.Item2 == null);
            if (kvps != null)
                for (int i = 0; i < kvps.Count(); i++)
                {
                    container.propertyDependencies.Remove(kvps.ElementAt(i));
                }
        }
        public static bool IsValidPair(object source, object presenter)
        {
            bool isInvalid = source == null || presenter == null || source == presenter || InvalidSourceObjects.Contains(source) || InvalidPresenterObjects.Contains(presenter);
            return !isInvalid;
        }

        /// <summary>
        /// inject trực tiếp không qua đăng kí tất cả các dependency có thể vào target
        /// </summary>
        /// <returns></returns>
        public static (object, object) HardInject(object source, object presenter, object containerKey, bool forceReinject = false, bool acceptSubclass = true)
        {
            var container = GetContainer(containerKey);

            ///bỏ qua nếu đã inject cặp này rồi
            if (forceReinject == false && container.injectionPair.Exists(x => x.Item1 == source && x.Item2 == presenter))
                return (presenter, containerKey);
            var presenterType = presenter.GetType();
            var sourceType = source.GetType();

            var presenterInjectFields = presenterType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.IsDefined(typeof(InjectAttribute)));

            if (presenterInjectFields == null || presenterInjectFields.Count() == 0)
            {
                InvalidPresenterObjects.Add(presenter);
            }
            else
            {
                var presenterFields = presenterInjectFields.Where(x => x.GetCustomAttributes<InjectAttribute>().Any(_ => sourceType == _.SourceType));
                if (acceptSubclass && (presenterFields == null || presenterFields.Count() == 0))
                {
                    presenterFields = presenterInjectFields.Where(x => x.GetCustomAttributes<InjectAttribute>().Any(_ => sourceType.IsSubclassOf(_.SourceType)));
                }
                if (presenterFields != null && presenterFields.Count() > 0)
                {
                    var sourceMethodInfos = sourceType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.IsDefined(typeof(MethodInjectionAttribute)));
                    var sourceFieldInfos = sourceType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.IsDefined(typeof(FieldInjectionAttribute)));
                    var sourcePropertiesInfo = sourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.IsDefined(typeof(FieldInjectionAttribute)));
                    bool sourceIsClassProvider = sourceType.IsDefined(typeof(ClassInjectionAttribute));
                    if (sourceMethodInfos == null && sourceFieldInfos == null && sourcePropertiesInfo == null && sourceIsClassProvider == false)
                    {
                        InvalidSourceObjects.Add(source);
                        return (null, null);
                    }

                    Exception e;
                    for (int i = 0; i < presenterFields.Count(); i++)
                    {
                        var presenterField = presenterFields.ElementAt(i);
                        if (sourceIsClassProvider && HardInjectClass(source, presenter, presenterField))
                            continue;

                        var attributes = presenterField.GetCustomAttributes<InjectAttribute>().Where(x => sourceType == x.SourceType || sourceType.IsSubclassOf(x.SourceType));
                        for (int j = 0; j < attributes.Count(); j++)
                        {
                            var attribute = attributes.ElementAt(j);
                            if (sourceMethodInfos != null)
                            {
                                var sourceMethodInfo = sourceMethodInfos.FirstOrDefault(x => x.Name == attribute.SourceName && x.DeclaringType == attribute.SourceType);
                                if (sourceMethodInfo != null && HardInjectMethod(source, sourceMethodInfo, presenter, presenterField, out e))
                                {
                                    //Debug.Log($"inject method {sourceType.Name}.{sourceMethodInfo.Name} to {presenterType.Name}.{presenterField.Name}");
                                    container.AddInjectionPair((source, presenter, presenterField));
                                    continue;
                                }
                            }

                            var sourceProperty = sourcePropertiesInfo.FirstOrDefault(x => x.Name == attribute.SourceName);
                            if (sourceProperty != null && HardInjectProperty(source, sourceProperty, presenter, presenterField, out e))
                            {
                                //Debug.Log($"inject property getter {sourceProperty.Name} to {presenterField.Name}");
                                container.AddInjectionPair((source, presenter, presenterField));
                                continue;
                            }

                            var sourceField = sourceFieldInfos.FirstOrDefault(x => x.Name == attribute.SourceName);
                            if (sourceField != null && HardInjectField(source, sourceField, presenter, presenterField))
                            {
                                //Debug.Log($"inject field {sourceField.Name} to {presenterField.Name}");
                                container.AddInjectionPair((source, presenter, presenterField));
                                continue;
                            }
                        }
                    }
                }

            }
            return (source, containerKey);
        }

        public static bool HardInjectClass(object source, object target, FieldInfo targetField)
        {
            if (targetField != null)
            {
                try
                {
                    targetField.SetValue(target, source);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    return false;
                }
            }
            else
                return false;
        }

        public static bool HardInjectProperty(object source, PropertyInfo sourceProperty, object presenter, FieldInfo presenterField, out Exception e)
        {
            e = null;
            if (source == null)
                return false;
            if (sourceProperty != null)
            {
                var getter = sourceProperty.GetGetMethod() ?? sourceProperty.GetGetMethod(true);
                ///check xem có property có getter nào mà có thể inject đc vào ko
                if (getter != null)
                {
                    return HardInjectMethod(source, getter, presenter, presenterField, out e);
                }
                return false;
            }
            return false;
        }

        public static bool HardInjectMethod(object source, MethodInfo sourceMethod, object presenter, FieldInfo presenterField, out Exception e)
        {
            e = null;
            object fieldValue = presenterField.GetValue(presenter);
            if (fieldValue is Delegate delegateInstance)
            {
                //Debug.Log($"{field.Name} is a delegate!");
                if (delegateInstance.Method.ReturnType != sourceMethod.ReturnType)
                {
                    Debug.LogError($"[Inject Error] {presenterField.DeclaringType.FullName}.{presenterField.Name} return type not match with source {sourceMethod.DeclaringType.FullName}.{sourceMethod.Name}");
                    return false;
                }
                if (delegateInstance.GetMethodInfo().GetParameters().Select(x => x.ParameterType).SequenceEqual(sourceMethod.GetParameters().Select(x => x.ParameterType)) == false)
                {
                    Debug.LogError($"[Inject Error] {presenterField.DeclaringType.FullName}.{presenterField.Name} argument types not match with source {sourceMethod.DeclaringType.FullName}.{sourceMethod.Name}");
                    return false;
                }
            }
            else
            {
                //Debug.LogWarning($"[Inject Warning] {presenterField.DeclaringType.FullName}.{presenterField.Name} does not have instantiated value, trying create blind action");
            }
            try
            {
                //var action = Delegate.CreateDelegate(field.FieldType, mInfo.Item1, mInfo.Item2.Name);
                var action = sourceMethod.CreateDelegate(presenterField.FieldType, source);
                presenterField.SetValue(presenter, action);
                return true;
            }
            catch (Exception ex)
            {
                e = ex;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceMethodName"></param>
        /// <param name="presenter"></param>
        /// <param name="targetFieldName"></param>
        /// <returns></returns>
        public static int HardInjectMethodOrProperty(object source, string sourceMethodName, object presenter, string targetFieldName)
        {
            var sourceType = source.GetType();
            var presenterType = presenter.GetType();
            var sourceMethodInfos = sourceType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.Name == sourceMethodName);
            var presenterField = presenterType.GetField(targetFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            int successOverloads = 0;
            bool success = false;
            Exception e = null;
            if (sourceMethodInfos != null && presenterField != null)
            {
                for (int j = 0; j < sourceMethodInfos.Count(); j++)
                {
                    var mInfo = sourceMethodInfos.ElementAt(j);
                    success = HardInjectMethod(source, mInfo, presenter, presenterField, out e);
                    if (success)
                    {
                        successOverloads++;
                        break;
                    }
                    else
                    {
                        if (e != null)
                        {
                            Debug.LogWarning($"Hard Inject {sourceType.Name}.{sourceMethodName} to {presenterType.Name}.{targetFieldName}: {e}");
                        }
                        continue;
                    }
                }
            }

            if (success == false)
            {
                var sourceProperty = sourceType.GetProperty(sourceMethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (sourceProperty != null)
                {
                    success = HardInjectProperty(source, sourceProperty, presenter, presenterField, out e);
                    if (success == false && e != null)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            return successOverloads;
        }

        public static bool HardInjectField(object source, FieldInfo sourceField, object target, FieldInfo targetField)
        {
            if (sourceField.FieldType == targetField.FieldType || sourceField.FieldType.IsSubclassOf(targetField.FieldType) == false)
                return false;

            targetField.SetValue(target, sourceField.GetValue(source));
            return true;
        }
        public static bool HardInjectField(object source, string sourceFieldName, object target, string targetFieldName)
        {
            var sType = source.GetType();
            var tType = target.GetType();
            if (sType.IsSubclassOf(tType) == false)
                return false;
            var sField = sType.GetField(sourceFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var tField = tType.GetField(targetFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (sField != null && tField != null)
            {
                tField.SetValue(target, sField.GetValue(source));
                return true;
            }

            else return false;
        }

        public class HorusDIInstallerComposition : List<object>
        { }

        public class HorusDIContainer
        {
            public List<(Type, object)> classDependencies = new List<(Type, object)>();
            public List<(Type, object, MethodInfo)> methodDependencies = new List<(Type, object, MethodInfo)>();
            public List<(Type, object, FieldInfo)> fieldDependencies = new List<(Type, object, FieldInfo)>();
            public List<(Type, object, PropertyInfo)> propertyDependencies = new List<(Type, object, PropertyInfo)>();

            public List<(Type, object, FieldInfo, bool, string)> waitingInject = new List<(Type, object, FieldInfo, bool, string)>();
            public List<(object, object, FieldInfo)> injectionPair = new List<(object, object, FieldInfo)>();//source - target

            public void AddInjectionPair((object, object, FieldInfo) pair)
            {
                ///remove pair injections trước đó trong trường hợp field được đánh dấu multiple attribute
                injectionPair.RemoveAll(x => x.Item2 == pair.Item2 && x.Item3 == pair.Item3);
                injectionPair.Add(pair);
            }
        }
    }
}
