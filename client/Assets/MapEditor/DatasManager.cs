using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public partial class DatasManager : GameCore.BaseMgr<DatasManager>
{
    Dictionary<Type, Dictionary<int, object>> datas = new Dictionary<Type, Dictionary<int, object>>();
    Dictionary<Type, Action> deserializeCallBacks = new Dictionary<Type, Action>();

	private int mLoadedFileCount = 0;
	private bool mIsLoading = false;
    private MemoryStream ms = new MemoryStream(500 * 1024);

    //加载所有数据
	public void LoadAllDatas(Action onFinish = null)
	{
		if(!mIsLoading)
		{
            ClearDatum();
            mIsLoading = true;
            DoLoadAllDatas();
		}
        else
        {
            if (mLoadedFileCount >= m_nTotalDatumCount)
            {
                if (null != onFinish)
                {
                    onFinish();
                    onFinish = null;
                }
            }
        }
	}

    //加载单条数据
    private void LoadData(string path, LoadResourcesCallback<TextAsset> cb)
    {
        mLoadedFileCount++;
        TextAsset asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(string.Format("Assets/Res/Resource/{0}.bytes", path));
        cb(asset);
    }
	
    //清理所有数据
	public void ClearDatum()
    {
        datas.Clear();
        deserializeCallBacks.Clear();
        mLoadedFileCount = 0;
        mIsLoading = false;
    }


    public T Deserialize<T> ( MemoryStream stream )
    {
        return ProtoBuf.Serializer.Deserialize<T>( stream );
    }

    //获取数据,根据类型和ID来获取
    public T GetData<T>(int id) where T : class
    {
        Dictionary<int, object> ret;
        IsLoaded(typeof(T));
        if (datas.TryGetValue(typeof(T), out ret))
        {
            object temp;
            ret.TryGetValue(id, out temp);
            return temp as T;
        }

        return default(T);
    }

    //获取某类型所有行也就是所有ID的数据
    public List<T> GetDatas<T>() where T : class
    {
        List<T> ret;
        Dictionary<int, object> temp;
        IsLoaded(typeof(T));
        if (datas.TryGetValue(typeof(T), out temp))
        {
            ret = new List<T>();
            Dictionary<int, object>.Enumerator _enumerator = temp.GetEnumerator();
            while (_enumerator.MoveNext())
            {
                ret.Add(_enumerator.Current.Value as T);
            }
            return ret;
        }
                                                                               
        return null;
    }

    //检查数据加载的回调是否执行了
    private void IsLoaded(Type t)
    {
        Action cb = null;
        if(deserializeCallBacks.TryGetValue(t,out cb))
        {
            if (cb != null)
                cb();
            deserializeCallBacks.Remove(t);
        }
    }
}
