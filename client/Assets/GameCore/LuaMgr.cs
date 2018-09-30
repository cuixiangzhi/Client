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
            string fullName = string.Format("{0}{1}", fileName, fileName.EndsWith(".lua") ? "" : ".lua");
            return GameCore.ResMgr.Instance.LoadBytes(fullName);
        }

        public override string FindFileError(string fileName)
        {
            return base.FindFileError(fileName);
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
        private static int luaopen_socket_core(System.IntPtr L)
        {
            return LuaDLL.luaopen_socket_core(L);
        }

        [AOT.MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int luaopen_mime_core(System.IntPtr L)
        {
            return LuaDLL.luaopen_mime_core(L);
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int Print_Error(IntPtr L)
        {
            try
            {
                int n = LuaDLL.lua_gettop(L);

                using (CString.Block())
                {
                    CString sb = CString.Alloc(256);
#if UNITY_EDITOR_
                    int line = LuaDLL.tolua_where(L, 1);
                    string filename = LuaDLL.lua_tostring(L, -1);
                    LuaDLL.lua_settop(L, n);

                    if (!filename.Contains("."))
                    {
                        sb.Append('[').Append(filename).Append(".lua:").Append(line).Append("]:");
                    }
                    else
                    {
                        sb.Append('[').Append(filename).Append(':').Append(line).Append("]:");
                    }
#endif

                    for (int i = 1; i <= n; i++)
                    {
                        if (i > 1) sb.Append("    ");

                        if (LuaDLL.lua_isstring(L, i) == 1)
                        {
                            sb.Append(LuaDLL.lua_tostring(L, i));
                        }
                        else if (LuaDLL.lua_isnil(L, i))
                        {
                            sb.Append("nil");
                        }
                        else if (LuaDLL.lua_isboolean(L, i))
                        {
                            sb.Append(LuaDLL.lua_toboolean(L, i) ? "true" : "false");
                        }
                        else
                        {
                            IntPtr p = LuaDLL.lua_topointer(L, i);

                            if (p == IntPtr.Zero)
                            {
                                sb.Append("nil");
                            }
                            else
                            {
                                sb.Append(LuaDLL.luaL_typename(L, i)).Append(":0x").Append(p.ToString("X"));
                            }
                        }
                    }

                    Debug.LogError(sb.ToString());            //200行与_line一致
                }
                return 0;
            }
            catch (Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
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

        private void OpenLog()
        {
            LuaDLL.tolua_pushcfunction(mLuaState.L, Print_Error);
            LuaDLL.lua_setglobal(mLuaState.L, "print_error");
        }

        public override void Init(GameObject owner)
        {
            base.Init(owner);
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
            //log函数
            OpenLog();
            //模式匹配库
            mLuaState.OpenLibs(LuaDLL.luaopen_lpeg);
            //位操作库
            mLuaState.OpenLibs(LuaDLL.luaopen_bit);
            //导出C# API
            LuaBinder.Bind(mLuaState);
            DelegateFactory.Init();

            //启动虚拟机
            mLuaState.Start();
        }

        public override void Update()
        {
            if (mLuaState.LuaUpdate(Time.deltaTime, Time.unscaledDeltaTime) != 0)
            {
                ThrowException();
            }

            mLuaState.LuaPop(1);
            mLuaState.Collect();
        }

        public override void LateUpdate()
        {
            if (mLuaState.LuaLateUpdate() != 0)
            {
                ThrowException();
            }

            mLuaState.LuaPop(1);
        }

        public override void FixedUpdate()
        {
            if (mLuaState.LuaFixedUpdate(Time.fixedDeltaTime) != 0)
            {
                ThrowException();
            }

            mLuaState.LuaPop(1);
        }

        public override void Exit()
        {
            base.Exit();
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

