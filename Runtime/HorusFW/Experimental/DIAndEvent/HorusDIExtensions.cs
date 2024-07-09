using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Runtime.CompilerServices;

namespace HorusFW.DI
{
    public static class HorusDIExtensions
    {
        public static (object, object) HardInjectMethodTo(this object source, string nameOfSourceMethod, object presenter, string nameOfPresenterField = null, object containerKey = null)
        {
            bool success = false;
            Exception e = null;
            if (HorusDI.IsValidPair(source, presenter))
            {
                if (string.IsNullOrEmpty(nameOfPresenterField) == false)
                {
                    int overloadSuccess = HorusDI.HardInjectMethodOrProperty(source, nameOfSourceMethod, presenter, nameOfPresenterField);
                    success = overloadSuccess > 0;
                }
                else
                {
                    var presenterFields = presenter.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.IsDefined(typeof(InjectAttribute)));
                    if (presenterFields.Count() > 0)
                    {
                        var methods = source.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.Name == nameOfSourceMethod && x.IsDefined(typeof(MethodInjectionAttribute)));
                        var sourceProperty = source.GetType().GetProperty(nameOfSourceMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                        for (int i = 0; i < presenterFields.Count(); i++)
                        {
                            var presenterField = presenterFields.ElementAt(i);

                            if (methods.Count() > 0)
                            {
                                for (int j = 0; j < methods.Count(); j++)
                                {
                                    MethodInfo method = methods.ElementAt(j);
                                    if (HorusDI.HardInjectMethod(source, method, presenter, presenterField, out e))
                                    {
                                        success = true;
                                        break;
                                    }
                                }
                            }

                            if(success == false)
                            {
                                success = sourceProperty != null && HorusDI.HardInjectProperty(source, sourceProperty, presenter, presenterField, out e);
                            }
                        }
                    }
                }
            }
            if (success)
                return (source, containerKey);
            else
            {
                if (e != null)
                    Debug.LogError(e);
                return (null, null);
            }
        }

        /// <summary>
        /// inject các member trong tuple, containerKey = member đầu tiên != null. Ví dụ: (a, b, c, d).MixInject();
        /// </summary>
        /// <param name="tuple"></param>
        /// <param name="containerKey"></param>
        /// <param name="forceReinject"></param>
        public static void MixInject(this ITuple tuple, object containerKey = null, bool forceReinject = false)
        {
            int s = -1;
            var objs = new object[tuple.Length].Select(x => tuple[++s]).Where(x => x != null).ToArray();
            if (objs != null)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    for (int j = 0; j < objs.Length; j++)
                    {
                        if (i == j)
                            continue;
                        objs[i].InjectTo(objs[j], containerKey ?? objs[0], forceReinject);
                    }

                    HorusDI.Inject(null, objs[i], forceReinject);
                }
            }
        }

        /// <summary>
        /// register và inject các component trên gameobject này cùng với các game object khác trong list truyền vào
        /// </summary>
        /// <param name="go"></param>
        /// <param name="includeChild"></param>
        /// <param name="includeInactive"></param>
        /// <param name="others"></param>
        public static void InjectComponentsWith(this GameObject go, bool includeChild, bool includeInactive, bool forceReinject, params GameObject[] others)
        {
            var components = new MonoBehaviour[0];
            if (includeChild)
            {
                components = go.GetComponentsInChildren<MonoBehaviour>(includeInactive);
            }
            else
            {
                components = go.GetComponents<MonoBehaviour>();
            }
            for (int i = 0; i < others.Length; i++)
            {
                if (includeChild)
                {
                    components = components.Concat(others[i].GetComponentsInChildren<MonoBehaviour>(includeInactive)).ToArray();
                }
                else
                {
                    components = components.Concat(others[i].GetComponents<MonoBehaviour>()).ToArray();
                }
            }

            var composition = HorusDI.Compose(go, components);
            for (int i = 0; i < components.Length; i++)
            {
                for (int j = 0; j < components.Length; j++)
                {
                    if (j == i)
                        continue;
                    ///inject với scope = go
                    composition.InjectTo(components[i], go, forceReinject).AddTo(go);
                }
                ///inject global singleton
                HorusDI.Inject(null, components[i], forceReinject);
            }
        }

        /// <summary>
        /// tự động tìm và register, inject các component gắn trên GameObject này với nhau
        /// </summary>
        /// <param name="go"></param>
        /// <param name="includeChild"></param>
        /// <param name="includeInactive"></param>
        public static void InjectComponents(this GameObject go, bool includeChild = false, bool includeInactive = false, bool forceReinject = false)
        {
            var components = new MonoBehaviour[0];
            if (includeChild)
            {
                components = go.GetComponentsInChildren<MonoBehaviour>(includeInactive);
            }
            else
            {
                components = go.GetComponents<MonoBehaviour>();
            }
            var composition = HorusDI.Compose(go, components);
            for (int i = 0; i < components.Length; i++)
            {
                for (int j = 0; j < components.Length; j++)
                {
                    if (j == i)
                        continue;
                    ///inject với scope = go
                    composition.InjectTo(components[j], go, forceReinject).AddTo(go);
                }
                ///inject global
                HorusDI.Inject(null, components[i], null, forceReinject);
            }
        }

        /// <summary>
        /// group các source lại thành 1 cụm installer
        /// </summary>
        /// <param name="captainSource"></param>
        /// <param name="memberSources"></param>
        /// <returns></returns>
        public static HorusDI.HorusDIInstallerComposition ComposeWith(this object captainSource, object containerKey, params object[] memberSources)
        {
            HorusDI.HorusDIInstallerComposition result = new HorusDI.HorusDIInstallerComposition();
            result.Add(captainSource);
            for (int i = 0; i < memberSources.Length; i++)
            {
                result.Add(memberSources[i]);
            }
            for (int i = 0; i < result.Count; i++)
            {
                HorusDI.Register(result[i], containerKey);
            }
            return result;
        }

        /// <summary>
        /// ví dụ: (a, b, c, d, e).InjectTo(f);
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="containerKey"></param>
        /// <param name="forceReinject"></param>
        /// <returns></returns>
        public static (HorusDI.HorusDIInstallerComposition, object) InjectTo(this ITuple source, object target, object containerKey = null, bool forceReinject = false)
        {
            int s = -1;
            var objs = new object[source.Length].Select(x => source[++s]).Where(x => x != null).ToArray();
            var composition = HorusDI.Compose(containerKey, objs);
            return composition.InjectTo(target, containerKey, forceReinject);
        }

        public static (object, object) InjectTo(this object source, object target, object containerKey = null, bool forceReinject = false)
        {
            if (HorusDI.IsValidPair(source, target) == false)
                return (null, null);
            var reg = HorusDI.Register(source, containerKey);
            HorusDI.HardInject(source, target, containerKey, forceReinject);
            return reg;
        }

        /// <summary>
        /// inject các source trong installer vào các target
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <returns>danh sách chứa cả source lẫn target </returns>
        public static (HorusDI.HorusDIInstallerComposition, object) InjectTo(this HorusDI.HorusDIInstallerComposition source, object target, object containerKey = null, bool forceReinject = false)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (HorusDI.IsValidPair(source[i], target) == false)
                    continue;
                HorusDI.HardInject(source[i], target, containerKey, forceReinject);
            }
            return (source, target);
        }

        public static (object, object) InjectDependency(this object injectObj, object containerKey = null, bool forceReinject = false)
        {
            return HorusDI.Inject(null, injectObj, containerKey, forceReinject);
        }
        public static (object, object) RegisterDependency(this object dependency, object containerKey = null)
        {
            return HorusDI.Register(dependency, containerKey);
        }
        public static void UnregisterDependency(this object dependency, object containerKey = null)
        {
            HorusDI.UnRegister(dependency, containerKey);
        }

        /// <summary>
        /// gỡ bỏ injection khi disposerObj bị destroy
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="disposerObj"></param>
        public static HorusDIDisposer AddTo(this (object, object) obj, MonoBehaviour disposerObj)
        {
            return AddTo(obj, disposerObj.gameObject);
        }

        /// <summary>
        /// gỡ bỏ injection khi disposerObj bị destroy
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="disposerObj"></param>
        public static HorusDIDisposer AddTo(this (object, object) obj, GameObject disposerObj)
        {
            if (obj.Item1 == null || disposerObj == null)
                return null;
            HorusDIDisposer disposer = disposerObj.GetComponent<HorusDIDisposer>() ?? disposerObj.AddComponent<HorusDIDisposer>();

            disposer.Add(obj.Item1, obj.Item2);
            return disposer;
        }

        /// <summary>
        /// <see href="https://stackoverflow.com/questions/3669970/compare-two-listt-objects-for-equality-ignoring-order"/> 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }
    }
}
