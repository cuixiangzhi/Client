﻿using UnityEngine;
using System;
using System.Collections.Generic;
using LuaInterface;

using BindType = ToLuaMenu.BindType;
using System.Reflection;

public static class CustomSettings
{
    public static string saveDir = Application.dataPath + "/GameLogic/ToLua/Source/";    
    public static string toluaBaseType = Application.dataPath + "/GameCore/GameLua/ToLua/BaseType/";    

    //导出时强制做为静态类的类型(注意customTypeList 还要添加这个类型才能导出)
    //unity 有些类作为sealed class, 其实完全等价于静态类
    public static List<Type> staticClassTypes = new List<Type>
    {        
        typeof(UnityEngine.Application),
        typeof(UnityEngine.SystemInfo),
    };

    //附加导出委托类型(在导出委托时, customTypeList 中牵扯的委托类型都会导出， 无需写在这里)
    public static DelegateType[] customDelegateList = 
    {        
        _DT(typeof(Action<int,LuaByteBuffer>)),
        _DT(typeof(Action<int,UnityEngine.Object>)),
    };

    //在这里添加你要导出注册到lua的类型列表
    public static BindType[] customTypeList =
    {                
        //引擎
        _GT(typeof(UnityEngine.Component)),
        _GT(typeof(UnityEngine.Behaviour)),
        _GT(typeof(UnityEngine.GameObject)),
        _GT(typeof(UnityEngine.Transform)),
        _GT(typeof(UnityEngine.Camera)),
        _GT(typeof(UnityEngine.MonoBehaviour)),

        //框架
        _GT(typeof(LuaDebugTool)),
        _GT(typeof(LuaValueInfo)),
        _GT(typeof(List<LuaValueInfo>)),
        _GT(typeof(GameCore.LogMgr)),
        _GT(typeof(GameCore.UIFollow)),
        _GT(typeof(GameCore.AssetManager)),
        _GT(typeof(GameCore.UIManager)),
        _GT(typeof(GameCore.UpdateManager)),
        _GT(typeof(GameCore.SceneManager)),
        _GT(typeof(GameCore.BehaviourUI)),
        _GT(typeof(GameCore.BehaviourAudio)),
        _GT(typeof(GameCore.BehaviourEffect)),
        _GT(typeof(GameCore.BehaviourModel)),     

        //逻辑
    };

    public static List<Type> dynamicList = new List<Type>()
    {

    };

    //重载函数，相同参数个数，相同位置out参数匹配出问题时, 需要强制匹配解决
    //使用方法参见例子14
    public static List<Type> outList = new List<Type>()
    {
        
    };
        
    //ngui优化，下面的类没有派生类，可以作为sealed class
    public static List<Type> sealedList = new List<Type>()
    {

    };

    public static BindType _GT(Type t)
    {
        return new BindType(t);
    }

    public static DelegateType _DT(Type t)
    {
        return new DelegateType(t);
    }    
}