using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageStarDesInfoWrapper
{
    public MapEditConfigData.StageStarDesInfo _data = null;
    public Object _bindObject = null;

    public StageStarDesInfoWrapper()
    {
        _data = new MapEditConfigData.StageStarDesInfo();
        _data.bindTarget = new MapEditConfigData.TriggerTargetInfo();

        _bindObject = new Object();
    }

    public StageStarDesInfoWrapper(MapEditConfigData.StageStarDesInfo info)
    {
        _data = info;

        _bindObject = new Object();
    }
}

public class StageStarDesEditData : MonoBehaviour
{

    List<StageStarDesInfoWrapper> _data = null;
    public List<StageStarDesInfoWrapper> GetWrapperData()
    {
        if(null == _data)
        {
            _data = new List<StageStarDesInfoWrapper>();
        }

        return _data;
    }

    public void SetData(List<MapEditConfigData.StageStarDesInfo> val)
    {
        if (null == _data)
        {
            _data = new List<StageStarDesInfoWrapper>();
        }
        _data.Clear();
        for(int i = 0 ; i < val.Count; i++)
        {
            StageStarDesInfoWrapper wr = new StageStarDesInfoWrapper(val[i]);
            _data.Add(wr);
        }
    }

    public List<MapEditConfigData.StageStarDesInfo> GetData()
    {
        List<MapEditConfigData.StageStarDesInfo> ret = new List<MapEditConfigData.StageStarDesInfo>();
        
        for(int i = 0 ; i < _data.Count; i++)
        {
            ret.Add(_data[i]._data);
        }

        return ret;
    }
}
