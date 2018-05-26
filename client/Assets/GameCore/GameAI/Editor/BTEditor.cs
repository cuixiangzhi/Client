using System;
using UnityEditor;
using UnityEngine;

namespace GameCore.AI.Editor
{
	public sealed class BTEditor
	{
		[MenuItem("Tools/BTEditor/打开行为树编辑器")]
		public static void OpenBTWindow()
		{
			EditorWindow.GetWindow<BTWindow>("BTWindow").Show();
		}
		//控制结点 多个后继结点,根据打断方式不同每帧做不同子树或者兄弟树条件计算
		//串行序列(and操作  顺序执行,遇到失败返回失败,全部成功返回成功) 
		//串行选择(or操作 顺序执行,遇到成功返回成功,全部失败返回失败)
		//并行序列(并行and操作 顺序执行,全部成功返回成功,有一个失败返回失败)
		//并行选择(并行or操作 顺序执行,有一个成功返回成功,全部失败返回失败)
		//随机选择(随机执行一颗子树,每颗子树有动态权值作为随机条件)
		//装饰结点 一个后继结点
		//取反 控制结果
		//失败 控制结果
		//成功 控制结果
		//重复执行直到返回成功 控制结果
		//重复执行直到返回失败 控制结果
		//重复执行N次 控制结果
		//条件结点 
		//收到指定事件
		//其它自定义条件
		//行为结点
		//可以外接子树
	}
}

