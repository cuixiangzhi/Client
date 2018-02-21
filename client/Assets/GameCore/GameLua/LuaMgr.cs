using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using System.IO;
using System;

namespace GameCore
{
    public static class LuaMgr
    {
        private static LuaState mLuaState = null;

        private static LuaFileReader mLuaReader = null;

        private class LuaFileReader : LuaFileUtils
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
                return AssetManager.LoadAsset(fileName);
#endif
            }

            public override string FindFileError(string fileName)
            {
                return string.Empty;
            }
        }

        [AOT.MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int luaopen_socket_core(System.IntPtr L)
        {
            return LuaDLL.luaopen_socket_core(L);
        }

        [AOT.MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int luaopen_mime_core(System.IntPtr L)
        {
            return LuaDLL.luaopen_mime_core(L);
        }

        private static void OpenLuaSocket()
        {
            mLuaState.BeginPreLoad();
            mLuaState.RegFunction("socket.core", luaopen_socket_core);
            mLuaState.RegFunction("mime.core", luaopen_mime_core);
            mLuaState.EndPreLoad();
        }

        private static void OpenCJson()
        {
            mLuaState.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
            mLuaState.OpenLibs(LuaDLL.luaopen_cjson);
            mLuaState.LuaSetField(-2, "cjson");

            mLuaState.OpenLibs(LuaDLL.luaopen_cjson_safe);
            mLuaState.LuaSetField(-2, "cjson.safe");
        }

        public static void Init()
        {
            //创建LUA文件读取器
            mLuaReader = new LuaFileReader();

            //创建LUA虚拟机
            mLuaState = new LuaState();

            //lua protobuf库
            mLuaState.OpenLibs(LuaDLL.luaopen_pb);
            //lua socket 和协议
            OpenLuaSocket();
            //json 库
            OpenCJson();
            //lua 模式匹配库
            mLuaState.OpenLibs(LuaDLL.luaopen_lpeg);
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            mLuaState.OpenLibs(LuaDLL.luaopen_bit);
#endif

            //导出C# API
            LuaBinder.Bind(mLuaState);
            DelegateFactory.Init();

            //启动虚拟机
            mLuaState.Start();

            //LUA逻辑入口
            mLuaState.DoFile("GameMain");
        }

        public static void Loop()
        {
            if (mLuaState.LuaUpdate(Time.deltaTime, Time.unscaledDeltaTime) != 0)
            {
                ThrowException();
            }

            mLuaState.LuaPop(1);
            mLuaState.Collect();
        }

        public static void LateLoop()
        {
            if (mLuaState.LuaLateUpdate() != 0)
            {
                ThrowException();
            }

            mLuaState.LuaPop(1);
        }

        public static void FixedLoop()
        {
            if (mLuaState.LuaFixedUpdate(Time.fixedDeltaTime) != 0)
            {
                ThrowException();
            }

            mLuaState.LuaPop(1);
        }

        public static void Exit()
        {
            mLuaState.DoFile("GameExit");
            mLuaState.Dispose();
            mLuaState = null;
            mLuaReader.Dispose();
            mLuaReader = null;
        }

        private static void ThrowException()
        {
            string error = mLuaState.LuaToString(-1);
            mLuaState.LuaPop(2);
            throw new LuaException(error, LuaException.GetLastError());
        }

        public static LuaState GetLuaState()
        {
            return mLuaState;
        }

        public static void DoFile(string fileName)
        {
            mLuaState.DoFile(fileName);
        }
    }
}

