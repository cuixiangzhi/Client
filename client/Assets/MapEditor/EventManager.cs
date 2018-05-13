/************************************************************************/
/* 事件管理器 
 * 
 * 事件定义的规范：
 * 1.父类必须为EventArg，无参数的事件可以从EventArg继承
 * 2.事件类名为"Event"打头，比如EventEntityCreate
 * 3.事件须详细注释，包括事件的功能，触发时间，参数类型，参数的涵义，事件成员变量的说明等
/************************************************************************/
using System;
using System.Collections.Generic;

[EventDeclare()]
public class EventTriggerNodeExecBegin : EventBase<EventTriggerNodeExecBegin>
{
    public int TriggerId = 0;
    public int NodeId = 0;
    public int CallerNodeId = 0;

    public EventTriggerNodeExecBegin reset(int _TriggerId, int _NodeId, int _CallerNodeId)
    {
        TriggerId = _TriggerId;
        NodeId = _NodeId;
        CallerNodeId = _CallerNodeId;

        return this;
    }
}

[EventDeclare()]
public class EventTriggerNodeExecFinish : EventBase<EventTriggerNodeExecFinish>
{
    public int TriggerId = 0;
    public int NodeId = 0;

    public EventTriggerNodeExecFinish reset(int _TriggerId, int _NodeId)
    {
        TriggerId = _TriggerId;
        NodeId = _NodeId;

        return this;
    }
}

public class EventManager : Singleton<EventManager>
{
    public delegate void EventDelegate<T>(T e) where T : EventArgs;

    readonly Dictionary<int, Dictionary<Type, Delegate>> single_delegates = new Dictionary<int, Dictionary<Type, Delegate>>();
    readonly Dictionary<Type, Delegate> multi_delegates = new Dictionary<Type, Delegate>();

    /// <summary>
    /// 监听事件
    /// </summary>
    /// <param name="listener"> 监听者</param>
    /// <param name="ID">可选参数，指定除了-1的其它ID的话，只有trigger时指定的相同ID时才被响应，一般指定ID用来只发给特定某个Entity</param>
    /// <returns>无</returns>
    public void AddListener<T>(EventDelegate<T> listener, int ID = -1) where T : EventArgs
    {
        if (ID != -1)
        {
            Delegate d;
            Dictionary<Type, Delegate> single;
            if (single_delegates.TryGetValue(ID, out single))
            {
                if (single.TryGetValue(typeof(T), out d))
                {
                    single[typeof(T)] = Delegate.Combine(d, listener);
                }
                else
                {
                    single[typeof(T)] = listener;
                }
            }
            else
            {
                single_delegates[ID] = new Dictionary<Type, Delegate>();
                single_delegates[ID][typeof(T)] = listener;
            }
        }
        else
        {
            Delegate del;
            if (multi_delegates.TryGetValue( typeof( T ), out del ))
            {
                multi_delegates[typeof( T )] = Delegate.Combine( del, listener );
            }
            else
            {
                multi_delegates[typeof( T )] = listener;
            }
        }
    }
    /// <summary>
    /// 移除事件监听者
    /// </summary>
    /// <param name="listener"> 监听者</param>
    /// <param name="ID">可选参数，-1为移除所有这个T类型的事件，指定ID则只移除注册过的对应ID事件</param>
    /// <returns>无</returns>
    public void RemoveListener<T>(EventDelegate<T> listener, int ID = -1) where T : EventArgs
    {
        if (ID != -1)
        {
            Delegate d;
            Dictionary<Type, Delegate> single;
            if (single_delegates.TryGetValue(ID, out single))
            {
                if (single.TryGetValue(typeof(T), out d))
                {
                    Delegate currentDel = Delegate.Remove(d, listener);

                    if (currentDel == null)
                    {
                        single_delegates[ID].Remove(typeof(T));
                    }
                    else
                    {
                        single_delegates[ID][typeof(T)] = currentDel;
                    }
                }
            }
        }
        else
        {
            Delegate dMulti;
            if (multi_delegates.TryGetValue( typeof( T ), out dMulti ))
            {
                Delegate currentDel = Delegate.Remove( dMulti, listener );

                if (currentDel == null)
                {
                    multi_delegates.Remove( typeof( T ) );
                }
                else
                {
                    multi_delegates[typeof( T )] = currentDel;
                }
            }
        }
    }
    /// <summary>
    /// 触发一个事件
    /// </summary>
    /// <param name="T"> 事件类型</param>
    /// <param name="ID">可选参数，指定除了-1的其它ID的话，只有trigger时指定的相同ID时才被响应，一般指定ID用来只发给特定某个Entity</param>
    /// <returns>无</returns>
    public void Trigger<T>(T e, int ID = -1) where T : EventBase<T>
    {
        if (e == null)
        {
            throw new ArgumentNullException();
        }

        if (ID != -1)
        {
            Delegate d;
            Dictionary<Type, Delegate> single;
            if (single_delegates.TryGetValue(ID, out single))
            {
                if (single.TryGetValue(typeof(T), out d))
                {
                    EventDelegate<T> callback = d as EventDelegate<T>;

                    if (callback != null)
                    {
                        callback(e);
                    }
                }
            }

            if (multi_delegates.TryGetValue( typeof( T ), out d ))
            {
                EventDelegate<T> callback = d as EventDelegate<T>;
                if (callback != null)
                {
                    callback( e );
                }
            }
            
        }
        else
        {
            Delegate d;
            if (multi_delegates.TryGetValue(typeof(T), out d))
            {
                EventDelegate<T> callback = d as EventDelegate<T>;
                if (callback != null)
                {
                    callback(e);
                }
            }
        }

        e.Recyle();
    }

    public bool HasRegister<T>(EventDelegate<T> listener, int ID = -1) where T : EventArgs
    {
        if (ID != -1)
        {
            Delegate d;
            Dictionary<Type, Delegate> single;
            if (single_delegates.TryGetValue(ID, out single))
            {
                if (single.TryGetValue(typeof(T), out d))
                {
                    return listener == d;
                }
            }
        }
        else
        {
            Type t = typeof(T);
            if(!multi_delegates.ContainsKey(t))
            {
                return false;
            }

            Delegate[] dels = multi_delegates[t].GetInvocationList();
            for(int i = 0 ; i < dels.Length; i++)
            {
                if (dels[i] == listener)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
