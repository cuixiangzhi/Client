/********************************************************************************
 *	创建人：	 
 *	创建时间：   2015-06-11   作废
 *
 *	功能说明： 测试使用
 *	
 *	修改记录：
*********************************************************************************/
using UnityEngine;
using System;
using System.Collections;

public class AssetBundleTask 
{
	public enum LoadType
	{
		Only_Download,
		Only_Loadlocal,
		Download_And_Loadlocal,
	}

	public enum LoadAsyncType
	{
		Load_Sync,
		Load_Async,
	}

	public string m_ResourceName;
	public LoadType m_AssetType;
	public LoadAsyncType m_LoadAssetType;
	//加载结束回调
	public Action<AssetBundleTask> m_LoadFinish;
	//查询进度回调
	public Action<float> m_Progress;
	//加载错误回调
	public Action<string> m_LoadError;

	//实体
	public UnityEngine.Object Obj;

	public AssetBundleTask(string pbName, Action<AssetBundleTask> end,LoadAsyncType loadtype = LoadAsyncType.Load_Sync, LoadType type = LoadType.Download_And_Loadlocal)
	{
		m_ResourceName = pbName;
		m_AssetType = type;
		m_LoadAssetType = loadtype;
		m_LoadFinish = end;
	}

	public void DispatchHandle()
	{

	}
}
