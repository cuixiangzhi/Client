using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public class EventDeclare : System.Attribute
{
	public EventDeclare(string desc)
	{
		m_desc = desc;
	}

    public EventDeclare()
    {
        m_desc = "";
    }

	public string m_desc;
}

public class EventBase<T> : EventArgs where T : class
{
	public static T Get()
	{
		return EventPool.GetEvent(typeof(T)) as T;
	}
	
	public void Recyle()
	{
		EventPool.RecyleEvent(this);
	}
}

public class EventPool
{
    static bool m_bInited = false;
	public static void InitPool()
	{
        m_bInited = true;
		if(m_event_pool == null)
		{
			m_event_pool = new Dictionary<Type, Queue<EventArgs>>();
		}
		m_event_pool.Clear();

		var classes = Assembly.GetExecutingAssembly().GetTypes();
		for (int i = 0; i < classes.Length; i++)
		{
			Type t = classes[i];
			
			object[] attributes = t.GetCustomAttributes(typeof(EventDeclare), false);
			if (attributes.Length > 0)
			{
				RegisterEvent(t);
			}
		}
	}

	public static EventArgs GetEvent(Type t)
	{
#if UNITY_EDITOR
        if(!m_bInited)
        {
            InitPool();
        }
#endif
		if(m_event_pool[t].Count == 0)
		{
			for(int i = 0 ; i < MAX_CACHE_EVENT_NUMBER; i++)
			{
				EventArgs evt = System.Activator.CreateInstance(t) as EventArgs;
				m_event_pool[t].Enqueue(evt);
			}
		}

		return m_event_pool[t].Dequeue();
	}

	public static void RecyleEvent(EventArgs e)
	{
		m_event_pool[e.GetType()].Enqueue(e);
	}

	private static void RegisterEvent(Type t)
	{
		if(!m_event_pool.ContainsKey(t))
		{
			m_event_pool[t] = new Queue<EventArgs>();
		}

		for(int i = 0 ; i < MAX_CACHE_EVENT_NUMBER; i++)
		{
			EventArgs evt = System.Activator.CreateInstance(t) as EventArgs;
			m_event_pool[t].Enqueue(evt);
		}
	}

	private static Dictionary<Type, Queue<EventArgs>> m_event_pool = null;
	private static int MAX_CACHE_EVENT_NUMBER = 5;
}
