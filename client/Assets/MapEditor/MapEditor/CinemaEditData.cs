using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CinemaEditData : IEditEventSender
{
    //private GameObject m_editSrcipt = new GameObject();
	
	public override TriggerTargetType ToTargetType()
	{
		return TriggerTargetType.StageScript;
	}

    public override int MinIdValue()
    {
        return MapEditorCons.PRESERVED_NUMBER;
    }

    public override System.Type GetBaseType()
    {
        return typeof(CinemaEditData);
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }

	new void Start()
	{
		base.Start();
		if(m_data != null)
		{
			name = m_data.name;
		}
	}

	private CameraPathDataPkg.CameraPathData m_data = null;
	public CameraPathDataPkg.CameraPathData Data
	{
        //get
        //{
        //          m_data = this.gameObject.GetComponent<CameraPath>().SavePathData();
        //	m_data.id = id;
        //	return m_data;
        //}
        get;set;
	}

    public void LoadData(CameraPathDataPkg.CameraPathData _data)
    {
  //      m_data = _data;
  //      this.gameObject.GetComponent<CameraPath>().LoadPathData(_data);
		//name = _data.name;
  //      id = _data.id;
    }

    public void SaveData(CameraPathDataPkg.CameraPathData _data)
    {
        m_data = _data;
		name = _data.name;
        id = m_data.id;
    }

}
