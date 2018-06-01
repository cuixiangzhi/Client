using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace GameCore.AI.Editor
{
    public sealed class NodeData
    {
        public int groupID;
        public string groupName;
        public int nodeID;
        public string nodeName;
        public string nodeTip;
    }

	public sealed class BTEditor
	{
		[MenuItem("Tools/BTEditor/打开行为树编辑器")]
		public static void OpenBTWindow()
		{
			EditorWindow.GetWindow<BTWindow>("BTWindow").Show();
		}

        public static List<NodeData> GetNode()
        {
            List<NodeData> ret = new List<NodeData>();
            DatasManager.Instance.LoadAllDatas();
            List<StageEventHandlerData.StageEventHandlerData> datas = DatasManager.Instance.GetDatas<StageEventHandlerData.StageEventHandlerData>();

            for (int i = 0; i < datas.Count; i++)
            {
                NodeData data = new NodeData();
                if(datas[i].Group == "触发器相关")
                {
                    data.groupID = 1;
                }
                else if(datas[i].Group == "功能相关")
                {
                    data.groupID = 2;
                }
                else if (datas[i].Group == "Entity相关")
                {
                    data.groupID = 3;
                }
                else if (datas[i].Group == "关卡相关")
                {
                    data.groupID = 4;
                }
                else if (datas[i].Group == "攻城关卡相关")
                {
                    data.groupID = 5;
                }
                else if (datas[i].Group == "UE相关")
                {
                    data.groupID = 6;
                }
                else if (datas[i].Group == "新手相关")
                {
                    data.groupID = 7;
                }
                else if (datas[i].Group == "新天网")
                {
                    data.groupID = 8;
                }
                data.groupName = datas[i].Group;
                data.nodeID = datas[i].ID;
                data.nodeName = datas[i].Name;
                data.nodeTip = datas[i].Des;
                ret.Add(data);
            }
            ret.Sort((a, b) =>
            {
                if (a.groupID != b.groupID)
                    return a.groupID.CompareTo(b.groupID);
                if (a.nodeID != b.nodeID)
                    return a.nodeID.CompareTo(b.nodeID);
                return -1;
            });
            return ret;
        }

        public static void AddNode(NodeData data,Vector2 position)
        {

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

