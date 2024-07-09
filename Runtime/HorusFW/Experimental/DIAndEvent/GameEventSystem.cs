using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

public class GameEventSystem : MonoBehaviour
{
    #region Properties
    private static List<Notifier> notifierList = new List<Notifier>();
    private static List<object> listeners = new List<object>();
    #endregion

    #region Orchestra

    private void Awake()
    {
        RegisterListener(this).AddTo(this);
    }

    private void OnDestroy()
    {
        Release();
    }

    /// <summary>
    /// call khi muốn release toàn bộ, call khi đổi account, chọn account
    /// </summary>
    /// 
    [EventListener(GameEventName.ResetAccount)]
    public static void Release()
    {
        for (int i = 0; i < notifierList.Count; i++)
        {
            notifierList[i].RemoveAllListeners();
        }
        notifierList.Clear();
        GC.Collect();
    }

    public static void EmitEvent(GameEventName evName, params object[] objs)
    {
        try
        {
            if (objs != null)
            {
                var paramsType = objs.Select(x => x == null ? null : x.GetType());
                EmitEventWithType(evName, paramsType, objs);
            }
            else
            {
                ///do hệ thống nhầm lẫn khi chỉ truyền duy nhất 1 object null vào params, nó sẽ nhận diện là array objs = null
                ///
                EmitEventWithType(evName, new Type[] { null }, new object[] { null });
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }


    public static void EmitEventWithType(GameEventName evName, IEnumerable<Type> paramsType, params object[] objs)
    {
        try
        {
            var notifier = notifierList.FirstOrDefault(x => x.evName == evName && paramsType.SequenceEqual(x.evArguments))
                ?? notifierList.FirstOrDefault(x => x.evName == evName && paramsType.SequenceEqualChild(x.evArguments));
            if (notifier != null)
                notifier.Invoke(objs);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }


    internal static void Remove(GameEventName evName, Delegate del)
    {
        var paramsType = del.GetMethodInfo().GetParameters().Select(x => x.ParameterType);
        var notifier = notifierList.FirstOrDefault(x => x.evName == evName && paramsType.SequenceEqual(x.evArguments))
            ?? notifierList.FirstOrDefault(x => x.evName == evName && paramsType.SequenceEqualChild(x.evArguments));
        if (notifier != null)
        {
            notifier.Remove(del);
        }
    }

    internal static List<(GameEventName, Delegate)> RegisterListener(object listener, GameEventName evName, string methodName, Type[] paramsType)
    {
        List<(GameEventName, Delegate)> result = null;
        Type lType = listener.GetType();
        MethodInfo method = lType.GetMethod(methodName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            paramsType,
            null);

        if (method != null)
        {
            result = new List<(GameEventName, Delegate)>();
            result.Add(RegisterListener(listener, evName, method));
        }

        return result;
    }

    internal static List<(GameEventName, Delegate)> RegisterListener(object listener)
    {
        if (listener == null || listeners.Contains(listener))
        {
            Debug.LogWarning($"cannot listen twice or listener is null!");
            return null;
        }
        listeners.Add(listener);
        List<(GameEventName, Delegate)> result = null;
        Type lType = listener.GetType();
        MethodInfo[] lMethods = lType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (lMethods.Length > 0)
        {
            foreach (MethodInfo method in lMethods)
            {
                var atts = method.GetCustomAttributes<EventListenerAttribute>();
                if (atts != null)
                {
                    int attCount = atts.Count();
                    for (int i = 0; i < attCount; i++)
                    {
                        var att = atts.ElementAt(i);
                        if (result == null)
                            result = new List<(GameEventName, Delegate)>();

                        result.Add(RegisterListener(listener, att.EventName, method));
                    }
                }
            }
        }
        return result;
    }

    private static (GameEventName, Delegate) RegisterListener(object listener, GameEventName evName, MethodInfo method)
    {
        var action = method.CreateDelegate(CreateDelegateType(method), listener);
        var paramsType = method.GetParameters().Select(x => x.ParameterType);
        var notifier = notifierList.FirstOrDefault(x => x.evName == evName && paramsType.SequenceEqual(x.evArguments))
            ?? notifierList.FirstOrDefault(x => x.evName == evName && paramsType.SequenceEqualChild(x.evArguments));
        if (notifier == null)
        {
            notifier = new Notifier(evName, paramsType);
            notifierList.Add(notifier);
        }
        notifier.Add(action);
        return (evName, action);
    }

    internal static void UnRegisterListener(object listener, string methodName, GameEventName evName, params Type[] argumentTypes)
    {
        Type lType = listener.GetType();
        var method = lType.GetMethod(methodName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            argumentTypes,
            null);
        if (method != null)
        {
            Remove(evName, method.CreateDelegate(CreateDelegateType(method), listener));
        }
    }

    internal static void UnRegisterListener(object listener)
    {
        if (listener == null || listeners.Contains(listener) == false)
            return;
        listeners.Remove(listener);
        Type lType = listener.GetType();
        MethodInfo[] lMethods = lType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (lMethods.Length > 0)
        {
            foreach (MethodInfo method in lMethods)
            {
                var atts = method.GetCustomAttributes<EventListenerAttribute>();
                if (atts != null)
                {
                    for (int i = 0; i < atts.Count(); i++)
                    {
                        Remove(atts.ElementAt(i).EventName, method.CreateDelegate(CreateDelegateType(method), listener));
                    }
                }
            }
        }
    }

    static Type CreateDelegateType(MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length < 1)
        {
            return typeof(Action);
        }
        else if (parameters.Length < 2)
        {
            return typeof(Action<>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 3)
        {
            return typeof(Action<,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 4)
        {
            return typeof(Action<,,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 5)
        {
            return typeof(Action<,,,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 6)
        {
            return typeof(Action<,,,,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 7)
        {
            return typeof(Action<,,,,,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 8)
        {
            return typeof(Action<,,,,,,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 9)
        {
            return typeof(Action<,,,,,,,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 10)
        {
            return typeof(Action<,,,,,,,,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 11)
        {
            return typeof(Action<,,,,,,,,,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 12)
        {
            return typeof(Action<,,,,,,,,,,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 13)
        {
            return typeof(Action<,,,,,,,,,,,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 14)
        {
            return typeof(Action<,,,,,,,,,,,,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 15)
        {
            return typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 16)
        {
            return typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        else if (parameters.Length < 17)
        {
            return typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
        }
        return typeof(Action);
    }
    #endregion

    #region Bootstrap
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoadRuntimeMethod()
    {
        GameObject obj = new GameObject("GameEventSystem");
        obj.AddComponent<GameEventSystem>();
        GameObject.DontDestroyOnLoad(obj);
    }
    #endregion

    #region Subclass
    public class Notifier
    {
        public GameEventName evName;
        public IEnumerable<Type> evArguments;

        private Delegate action;

        public Notifier(GameEventName evName, IEnumerable<Type> evArguments)
        {
            this.evName = evName;
            this.evArguments = evArguments;
        }

        public void Add(Delegate act)
        {
            action = Delegate.Combine(action, act);
        }

        public void Invoke(params object[] objs)
        {
            action?.DynamicInvoke(objs);
        }

        public void Remove(Delegate act)
        {
            action = Delegate.Remove(action, act);
        }

        public void RemoveAllListeners()
        {
            if (action != null)
            {
                var del = action.GetInvocationList();
                for (int i = 0; i < del.Length; i++)
                {
                    action = Delegate.RemoveAll(action, del[i]);
                }
            }
        }
    }

    #endregion
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class EventListenerAttribute : System.Attribute
{
    public GameEventName EventName;
    public EventListenerAttribute(GameEventName eventName)
    {
        EventName = eventName;
    }
}

public static class GameEventSystemExtension
{
    public static Delegate AddListener(this Delegate source, Action additional)
    {
        source = Delegate.Combine(source, additional);
        return source;
    }

    public static Delegate RemoveListener(this Delegate source, Action additional)
    {
        source = Delegate.Remove(source, additional);
        return source;
    }

    public static void UnregisterEvent(this object listener, GameEventName evName, string methodName, params Type[] argumentTypes)
    {
        GameEventSystem.UnRegisterListener(listener, methodName, evName, argumentTypes);
    }

    public static bool SequenceEqualChild(this IEnumerable<Type> left, IEnumerable<Type> right)
    {
        int c = left.Count();
        if (c != right.Count())
            return false;
        for (int i = 0; i < c; i++)
        {
            var lType = left.ElementAt(i);
            var rType = right.ElementAt(i);
            if (lType != null && lType != rType && lType.IsSubclassOf(rType) == false)
            {
                return false;
            }
        }
        return true;
    }

    public static List<(GameEventName, Delegate)> ListenEvent(this object listener, GameEventName evName, string methodName, params Type[] argumentTypes)
    {
        return GameEventSystem.RegisterListener(listener, evName, methodName, argumentTypes);
    }

    public static List<(GameEventName, Delegate)> ListenEvent(this object listener)
    {
        return GameEventSystem.RegisterListener(listener);
    }

    /// <summary>
    /// kết thúc lắng nghe khi disposerObj bị destroy
    /// </summary>
    /// <param name="notifier"></param>
    /// <param name="disposerObj"></param>
    /// <returns></returns>
    public static int AddTo(this List<(GameEventName, Delegate)> notifier, MonoBehaviour disposerObj)
    {
        return AddTo(notifier, disposerObj.gameObject);
    }

    /// <summary>
    /// kết thúc lắng nghe khi disposerObj bị destroy
    /// </summary>
    /// <param name="notifier"></param>
    /// <param name="disposerObj"></param>
    /// <returns></returns>
    public static int AddTo(this List<(GameEventName, Delegate)> notifier, GameObject disposerObj)
    {
        if (notifier == null || notifier.Count == 0 || disposerObj == null)
            return 0;
        GameEventSystemDisposer disposer = disposerObj.GetComponent<GameEventSystemDisposer>() ?? disposerObj.AddComponent<GameEventSystemDisposer>();
        if (notifier.Count > 0)
        {
            disposer.Listeners.AddRange(notifier);
        }
        //for (int i = 0; i < notifier.Count; i++)
        //{
        //    disposer.Add(notifier[i]);
        //}
        return notifier.Count;
    }

    public static void EmitEvent(this object emitter, GameEventName evName, params object[] objs)
    {
        GameEventSystem.EmitEvent(evName, objs);
    }
    public static void EmitEventWithType(this object emitter, GameEventName evName, params (object, Type)[] objs)
    {
        GameEventSystem.EmitEventWithType(evName, objs.Select(x => x.Item2), Array.ConvertAll(objs, x => x.Item1));
    }

    public static void UnregisterEventListener(this object listener)
    {
        GameEventSystem.UnRegisterListener(listener);
    }
}

public class GameEventSystemDisposer : MonoBehaviour
{
    public readonly List<(GameEventName, Delegate)> Listeners = new List<(GameEventName, Delegate)>();

    private void OnDestroy()
    {
        for (int i = 0; i < Listeners.Count; i++)
        {
            GameEventSystem.Remove(Listeners[i].Item1, Listeners[i].Item2);
        }
    }
}