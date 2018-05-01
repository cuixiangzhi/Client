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
using System.Collections.Generic;
public class CoroutingProvide : MonoBehaviour {

	/// <summary>
	/// Coroutine provider. 一次只能有一个Coroutine在工作
	/// </summary>
	private static CoroutingProvide _current;
	public static CoroutingProvide GetInstance() 
	{
		Initialize();
		return _current;
	}

    public WWW www = null;

    public string warning = "";

	void Awake() 
	{
		_current = this;
		initialized = true;
	}

    void OnGUI()
    {
        if (www != null)
        {
            GUILayout.Label("www+++++++" + www.progress.ToString());
        }
        if (!string.IsNullOrEmpty(warning))
        {
            GUILayout.Space(100f);
            GUILayout.Label(warning);
        }

    }

	private static bool initialized = false;
	static void Initialize()
	{
		if (!initialized)
		{
			if(!Application.isPlaying)
				return;
			initialized = true;
			GameObject g = new GameObject("Corountine Provider");
			//GameObject gobal = GameObject.FindGameObjectWithTag("Gobal");
			//if(gobal != null) g.transform.parent = gobal.transform;
			_current = g.AddComponent<CoroutingProvide>();
			DontDestroyOnLoad(g);
		}
	}
		
}
