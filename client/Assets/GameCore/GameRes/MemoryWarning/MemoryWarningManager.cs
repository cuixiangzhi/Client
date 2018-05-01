using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace TLStudio.Plugin
{
	// 警告等级随数值增加而增加
	public enum MemoryWarningLevel
	{
		// 当我们的应用程序真正运行时
		TRIM_MEMORY_RUNNING_MODERATE = 5, // 内存不足(后台进程超过5个)
		TRIM_MEMORY_RUNNING_LOW = 10, // 内存不足(后台进程不足5个)
		TRIM_MEMORY_RUNNING_CRITICAL = 15, // 内存不足(后台进程不足3个)
		// 用户点击了Home键或者Back键导致我们的应用程序的UI界面不可见
		TRIM_MEMORY_UI_HIDDEN = 20,
		// 当我们的应用程序在后台缓存时
		TRIM_MEMORY_BACKGROUND = 40, // 内存不足，并且该进程是后台进程
		TRIM_MEMORY_MODERATE = 60, // 内存不足，并且该进程在后台进程列表的中部
		TRIM_MEMORY_COMPLETE = 80, // 内存不足，并且该进程在后台进程列表最后一个，马上就要被清理
	};

	public class MemoryWarningManager : MonoBehaviour
	{
		public delegate void ReceiveMemWarningHandler(int level);
		public static event ReceiveMemWarningHandler On_ReceiveMemWarning;
#if !UNITY_EDITOR
		void Start()
		{
#if UNITY_ANDROID
            AndroidJavaObject _class = new AndroidJavaObject("com.TLStudio.Plugin.MemoryWarning.MemoryWarningCallback");
			_class.CallStatic("CreateInstance");
			_class.CallStatic("SetGameObjectName", gameObject.name);
#endif

#if UNITY_IPHONE
			SetGameObjectName(gameObject.name);
#endif

		}

#if UNITY_IPHONE
		[DllImport("__Internal")]
		private static extern void SetGameObjectName(string name);

		// Tells the delegate when the app is about to terminate.
		public void applicationWillTerminate()
		{

		}
		// Tells the delegate that the app is now in the background.
		public void applicationDidEnterBackground()
		{

		}
#endif

		//Function is called when a lowMemoryWarning is received
		public void ReceivedMemoryWarning(string message)
		{
			int level = 0;

			if (!string.IsNullOrEmpty(message))
			{
				level = int.Parse(message);
			}

#if UNITY_ANDROID
			// 可能会开始根据LRU缓存规则来去杀死后台进程
			if (level == (int)MemoryWarningLevel.TRIM_MEMORY_RUNNING_MODERATE)
			{
				
			}
			// 系统已经根据LRU缓存规则杀掉了大部分缓存的进程
			else if (level == (int)MemoryWarningLevel.TRIM_MEMORY_RUNNING_CRITICAL)
			{
	
			}
			// 我们的应用程序在后台进程列表最后一个，马上就要被清理
			else if (level == (int)MemoryWarningLevel.TRIM_MEMORY_COMPLETE)
			{
	
			}
#endif

			if (On_ReceiveMemWarning != null)
			{
				On_ReceiveMemWarning(level);
			}
		}
#endif
    }
}