using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveDirectionGuidEditData : MonoBehaviour
{
    private List<MapEditConfigData.MoveDirectionGuidData> _data = null;
    public List<MapEditConfigData.MoveDirectionGuidData> Data
    {
        get
        {
            if(null == _data)
            {
                _data = new List<MapEditConfigData.MoveDirectionGuidData>();
            }

            return _data;
        }

        set
        {
            _data = value;
        }
    }
}
