using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using System.IO;
using System;

namespace LuaInterface
{
    public sealed class LuaFileReader : LuaFileUtils
    {
        public LuaFileReader()
        {
            instance = this;
            beZip = false;
        }

        public override LuaByteBuffer ReadFile(string fileName)
        {
#if UNITY_EDITOR
            return base.ReadFile(fileName);
#else
                //读取LUA字节码               
                return GameCore.ResMgr.LoadBytes(fileName);
#endif
        }

        public override string FindFileError(string fileName)
        {
            return string.Empty;
        }
    }
}

namespace GameCore
{
    public sealed class LuaMgr : BaseMgr<LuaMgr>
    {
        private LuaState mLuaState = null;

        private LuaFileReader mLuaReader = null;

        [AOT.MonoPInvokeCallback(typeof(LuaCSFunction))]
        private int luaopen_socket_core(System.IntPtr L)
        {
            return LuaDLL.luaopen_socket_core(L);
        }

        [AOT.MonoPInvokeCallback(typeof(LuaCSFunction))]
        private int luaopen_mime_core(System.IntPtr L)
        {
            return LuaDLL.luaopen_mime_core(L);
        }

        private void OpenLuaSocket()
        {
            mLuaState.BeginPreLoad();
            mLuaState.RegFunction("socket.core", luaopen_socket_core);
            mLuaState.RegFunction("mime.core", luaopen_mime_core);
            mLuaState.EndPreLoad();
        }

        private void OpenCJson()
        {
            mLuaState.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
            mLuaState.OpenLibs(LuaDLL.luaopen_cjson);
            mLuaState.LuaSetField(-2, "cjson");

            mLuaState.OpenLibs(LuaDLL.luaopen_cjson_safe);
            mLuaState.LuaSetField(-2, "cjson.safe");
        }

        public void Init()
        {
            //创建LUA文件读取器
            mLuaReader = new LuaFileReader();
            //创建LUA虚拟机
            mLuaState = new LuaState();

            //protobuf库
            mLuaState.OpenLibs(LuaDLL.luaopen_pb);
            //socket库
            OpenLuaSocket();
            //json库
            OpenCJson();
            //模式匹配库
            mLuaState.OpenLibs(LuaDLL.luaopen_lpeg);
            //位操作库
            mLuaState.OpenLibs(LuaDLL.luaopen_bit);
            //导出C# API
            LuaBinder.Bind(mLuaState);
            DelegateFactory.Init();

            //启动虚拟机
            mLuaState.Start();

            //LUA逻辑入口
            mLuaState.DoFile("GameMain");
            CallLuaFunc("GameMain.GameInit");
        }

        public void Loop()
        {
            if (mLuaState.LuaUpdate(Time.deltaTime, Time.unscaledDeltaTime) != 0)
            {
                ThrowException();
            }

            mLuaState.LuaPop(1);
            mLuaState.Collect();
        }

        public void LateLoop()
        {
            if (mLuaState.LuaLateUpdate() != 0)
            {
                ThrowException();
            }

            mLuaState.LuaPop(1);
        }

        public void FixedLoop()
        {
            if (mLuaState.LuaFixedUpdate(Time.fixedDeltaTime) != 0)
            {
                ThrowException();
            }

            mLuaState.LuaPop(1);
        }

        public void Exit()
        {
            CallLuaFunc("GameMain.GameQuit");
            mLuaState.Dispose();
            mLuaState = null;
            mLuaReader.Dispose();
            mLuaReader = null;
        }

        private void ThrowException()
        {
            string error = mLuaState.LuaToString(-1);
            mLuaState.LuaPop(2);
            throw new LuaException(error, LuaException.GetLastError());
        }

        public LuaState GetLuaState()
        {
            return mLuaState;
        }

        public void DoFile(string fileName)
        {
            mLuaState.DoFile(fileName);
        }

        public void CallLuaFunc(string funcName)
        {
            LuaFunction func = mLuaState.GetFunction(funcName);
            if (func != null)
            {
                func.Call();
            }
        }
    }
}

