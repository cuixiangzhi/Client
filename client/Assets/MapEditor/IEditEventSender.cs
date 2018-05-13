using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class IEditEventSender : MonoBehaviour
{
	public int id;
	public new string name = "";

	//[SerializeField]
    //Vector3 worldPos = Vector3.zero;

	public virtual int ID
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

    protected virtual void OnHiracheyChanged()
    {
    }

    private Transform m_orgParent;

    public void OnUpdate()
    {
        //worldPos = this.transform.position;
        if(transform.parent != m_orgParent)
        {
            OnHiracheyChanged();
            m_orgParent = transform.parent;
        }
    }

	public void Start()
	{
        m_orgParent = transform.parent;
        if (transform.parent == null)
		{
			return;
		}

        IEditEventSender conflict = null;

        int maxId = 0;
        for (int i = 0; i < transform.parent.childCount; i++)
		{
            IEditEventSender other = transform.parent.GetChild(i).GetComponent<IEditEventSender>();
            if (null == other || other == this)
                continue;

            if (maxId < other.id)
            {
                maxId = other.id;
            }

            if (this != other 
                && this.id == other.id
                && this.ToTargetType() == other.ToTargetType())
			{
                conflict = other;
                break;
			}
		}

		if(conflict != null)
		{
			transform.name += "_副本";

            IEditEventSender[] childs = transform.GetComponentsInChildren<IEditEventSender>();
            IEditEventSender[] conflictChilds = conflict.GetComponentsInChildren<IEditEventSender>();
            if (childs != null 
                && conflictChilds != null 
                && childs.Length == conflictChilds.Length)
            {
                for (int i = 0; i < childs.Length; i++)
                {
                    childs[i].OnClone(conflictChilds[i]);
                }
            }
		}
	}

    public virtual int MinIdValue()
    {
        return 0;
    }

    public virtual System.Type GetBaseType()
    {
        return this.GetType();
    }

    protected virtual void OnClone(IEditEventSender parent)
    {
    }

	public virtual TriggerTargetType ToTargetType() { return TriggerTargetType.None; }
}
