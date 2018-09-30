using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using LuaInterface;
using System;
using UnityEngine.Networking;

namespace GameCore
{
    public class NetMgr : BaseMgr<NetMgr>
    {
        public class SocketBuffer
        {
            public bool mInit = false;
            public byte[] mBuffer;
            public int mHead = 0;
            public int mTail = 0;
            public int mBufferLen = 0;
            public Socket mSocket;

            public void Init(Socket socket)
            {
                mSocket = socket;
                if(!mInit)
                {
                    mInit = true;
                    mHead = 0;
                    mTail = 0;
                    mBuffer = new byte[128 * 1024];
                    mBufferLen = mBuffer.Length;
                }
            }

            public void Exit()
            {
                mHead = 0;
                mTail = 0;
            }

            public int Length()
            {
                if (mHead < mTail)
                    return mTail - mHead;

                else if (mHead > mTail)
                    return mBufferLen - mHead + mTail;

                return 0;
            }

            public void Expand(int size)
            {
                size = Mathf.Max(size, mBufferLen >> 1);
                int newBufferLen = mBufferLen + size;
                int len = Length();
                byte[] newBuffer = new byte[newBufferLen];
                if (mHead < mTail)
                {
                    Array.Copy(mBuffer, mHead, newBuffer, 0, mTail - mHead);
                }
                else if (mHead > mTail)
                {
                    Array.Copy(mBuffer, mHead, newBuffer, 0, mBufferLen - mHead);
                    Array.Copy(mBuffer, 0, newBuffer, mBufferLen - mHead, mTail);
                }

                mBuffer = newBuffer;
                mBufferLen = newBufferLen;
                mHead = 0;
                mTail = len;
            }

            public bool Peek(byte[] buffer, int len)
            {
                if (len == 0 || len > Length())
                    return false;
                if (mHead < mTail)
                {
                    Array.Copy(mBuffer, mHead, buffer, 0, len);
                }
                else
                {
                    int rightLen = mBufferLen - mHead;
                    if (len <= rightLen)
                    {
                        Array.Copy(mBuffer, mHead, buffer, 0, len);
                    }
                    else
                    {
                        Array.Copy(mBuffer, mHead, buffer, 0, rightLen);
                        Array.Copy(mBuffer, 0, buffer, rightLen, len - rightLen);
                    }
                }
                return true;
            }

            public void Skip(int len)
            {
                if (len == 0 || len > Length())
                    return;
                mHead = (mHead + len) % mBufferLen;
            }

            public void Read(byte[] buffer, int len)
            {
                if (len == 0 || len > Length())
                    return;
                if (mHead < mTail)
                {
                    Array.Copy(mBuffer, mHead, buffer, 0, len);
                }
                else
                {
                    int rightLen = mBufferLen - mHead;
                    if (len <= rightLen)
                    {
                        Array.Copy(mBuffer, mHead, buffer, 0, len);
                    }
                    else
                    {
                        Array.Copy(mBuffer, mHead, buffer, 0, rightLen);
                        Array.Copy(mBuffer, 0, buffer, rightLen, len - rightLen);
                    }
                }

                mHead = (mHead + len) % mBufferLen;
            }

            public void Write(byte[] buffer,int len)
            {
                //获取空闲字节数
                int nFree = ((mHead <= mTail) ? (mBufferLen - mTail + mHead - 1) : (mHead - mTail - 1));
                //扩容
                if (len >= nFree)
                {
                    Expand(len - nFree + 1);
                }

                if (mHead <= mTail)
                {
                    if (mHead == 0)
                    {
                        nFree = mBufferLen - mTail - 1;
                        Array.Copy(buffer, 0, mBuffer, mTail, len);
                    }
                    else
                    {
                        nFree = mBufferLen - mTail;
                        if (len <= nFree)
                        {
                            Array.Copy(buffer, 0, mBuffer, mTail, len);
                        }
                        else
                        {
                            Array.Copy(buffer, 0, mBuffer, mTail, nFree);
                            Array.Copy(buffer, nFree, mBuffer, 0, len - nFree);
                        }
                    }
                }
                else
                {
                    Array.Copy(buffer, 0, mBuffer, mTail, len);
                }

                mTail = (mTail + len) % mBufferLen;
            }

            public void Fill()
            {
                int nFilled = 0;
                int nReceived = 0;
                int nFree = 0;

                if (mHead <= mTail)
                {
                    if (mHead == 0)
                    {
                        //读取缓冲区可容纳的字节数据
                        nReceived = 0;
                        nFree = mBufferLen - mTail - 1;
                        if (nFree != 0)
                        {
                            nReceived = mSocket.Receive(mBuffer, mTail, nFree, SocketFlags.None);
                            if (nReceived <= 0) return;
                            mTail += nReceived;
                            nFilled += nReceived;
                        }

                        //缓冲区不足,扩容后继续读取
                        if (nReceived == nFree)
                        {
                            int available = mSocket.Available;
                            if (available > 0)
                            {
                                Expand(available + 1);
                                nReceived = mSocket.Receive(mBuffer, mTail, available, SocketFlags.None);
                                if (nReceived <= 0) return;
                                mTail += nReceived;
                                nFilled += nReceived;
                            }
                        }
                    }
                    else
                    {
                        //读取缓冲区可容纳的字节数据,保留一个字节作为Tail位置
                        nFree = mBufferLen - mTail;
                        nReceived = mSocket.Receive(mBuffer, mTail, nFree, SocketFlags.None);
                        if (nReceived <= 0) return;
                        //循环BUFFER,修正Tail位置,读取字节数等于剩余字节数时Tail移动到初始位置
                        mTail = (mTail + nReceived) % mBufferLen;
                        nFilled += nReceived;
                        //读取字节数等于剩余字节数,可能有数据未读取完
                        if (nReceived == nFree)
                        {
                            nReceived = 0;
                            nFree = mHead - 1;
                            if (nFree != 0)
                            {
                                nReceived = mSocket.Receive(mBuffer, 0, nFree, SocketFlags.None);
                                if (nReceived <= 0) return;

                                mTail += nReceived;
                                nFilled += nReceived;
                            }
                            //读取字节数等于剩余字节数,检查是否有额外数据未读,扩容
                            if (nReceived == nFree)
                            {
                                int available = mSocket.Available;
                                if (available > 0)
                                {
                                    Expand(available + 1);
                                    nReceived = mSocket.Receive(mBuffer, mTail, available, SocketFlags.None);
                                    if (nReceived <= 0) return;

                                    mTail += nReceived;
                                    nFilled += nReceived;
                                }
                            }
                        }
                    }
                }
                else
                {
                    nReceived = 0;
                    nFree = mHead - mTail - 1;
                    if (nFree != 0)
                    {
                        nReceived = mSocket.Receive(mBuffer, mTail, nFree, SocketFlags.None);
                        if (nReceived <= 0) return;

                        mTail += nReceived;
                        nFilled += nReceived;
                    }
                    if (nReceived == nFree)
                    {
                        int available = mSocket.Available;
                        if (available > 0)
                        {
                            Expand(available + 1);
                            nReceived = mSocket.Receive(mBuffer, mTail, available, SocketFlags.None);
                            if (nReceived <= 0) return;

                            mTail += nReceived;
                            nFilled += nReceived;
                        }
                    }
                }
            }

            public void Flush()
            {
                int nFlushed = 0;
                int nSent = 0;
                int nLeft;
                if (mHead < mTail)
                {
                    nLeft = mTail - mHead;
                    while (nLeft > 0)
                    {
                        nSent = mSocket.Send(mBuffer, mHead, nLeft, SocketFlags.None);
                        if (nSent <= 0) return;

                        nFlushed += nSent;
                        nLeft -= nSent;
                        mHead += nSent;
                    }
                }
                else if (mHead > mTail)
                {
                    nLeft = mBufferLen - mHead;

                    while (nLeft > 0)
                    {
                        nSent = mSocket.Send(mBuffer, mHead, nLeft, SocketFlags.None);
                        if (nSent <= 0) return;

                        nFlushed += nSent;
                        nLeft -= nSent;
                        mHead += nSent;
                    }
                    mHead = 0;
                    nLeft = mTail;
                    while (nLeft > 0)
                    {
                        nSent = mSocket.Send(mBuffer, mHead, nLeft, SocketFlags.None);
                        if (nSent <= 0) return;
                        nFlushed += nSent;
                        nLeft -= nSent;
                        mHead += nSent;
                    }
                }
                if (nSent > 0)
                {
                    mHead += nSent;
                }

                mHead = 0;
                mTail = 0;
                return;
            }
        }

        public class SocketObject
        {
            public enum NET_EVENT
            {
                CONNECT = 1,
                DISCONNECT = 2,
                ERROR = 3,
                MESSAGE = 10,
            }

            public Socket mSocket;
            public int mSocketIndex = -1;
            public bool mConnect = false;
            public bool mConnecting = false;
            public SocketBuffer mRecvBuffer;
            public SocketBuffer mSendBuffer;
            public LuaFunction mLuaFunction;
            public int mMaxMsgPerFrame = 0;
            public byte[] mPackageHeader = new byte[8];
            public byte[] mPackageData = new byte[128 * 1024];
            public int MSG_HEADER_LEN = 7;

            public SocketObject(int socketIndex,LuaFunction luaFunc, int maxMsgPerFrame)
            {
                mSocketIndex = socketIndex;
                mLuaFunction = luaFunc;

                mRecvBuffer = new SocketBuffer();
                mSendBuffer = new SocketBuffer();

                mMaxMsgPerFrame = maxMsgPerFrame;
            }

            public void Connect(string ip, int port)
            {
                try
                {
                    if (mConnecting || (mSocket != null && mSocket.Connected))
                    {
                        Close();
                    }
                    mConnecting = true;
                    mConnect = false;

                    mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    mSocket.Blocking = true;
                    mSocket.Connect(IPAddress.Parse(ip), port);
                    mSocket.Blocking = false;
                    mSocket.LingerState.Enabled = false;
                    mRecvBuffer.Init(mSocket);
                    mSendBuffer.Init(mSocket);
                }
                catch(Exception e)
                {
                    OnError(e.Message);
                }
            }

            public void Close()
            {
                try
                {
                    mConnect = false;
                    mConnecting = false;
                    mRecvBuffer.Exit();
                    mSendBuffer.Exit();

                    if(mSocket != null)
                    {
                        mSocket.Blocking = true;
                        mSocket.Close();
                        mSocket = null;
                    }
                }
                catch(Exception e)
                {
                    OnError(e.Message);
                }
            }

            public void Update()
            {
                if(mSocket != null)
                {
                    ProcessExceptions();
                    ProcessConnect();
                    ProcessInput();
                    ProcessOutput();
                    ProcessCommand();
                }
            }

            public void Send(int tid, int gid, int uid, byte[] data)
            {
                mPackageData[0] = (byte)tid;
                mPackageData[5] = (byte)gid;
                mPackageData[6] = (byte)uid;
                convert(mPackageData, 1, data.Length);
                Array.Copy(data, 0, mPackageData, MSG_HEADER_LEN, data.Length);
                mSendBuffer.Write(mPackageData, MSG_HEADER_LEN + data.Length);
            }

            private void ProcessConnect()
            {
                //网络状态发生变化
                if (mSocket != null && mConnect != mSocket.Connected)
                {
                    mConnecting = false;
                    mConnect = mSocket.Connected;
                    OnConnect();
                }
            }

            private void ProcessInput()
            {
                if (mSocket != null && mSocket.Connected && mSocket.Poll(0, SelectMode.SelectRead))
                {
                    mRecvBuffer.Fill();
                }
            }

            private void ProcessOutput()
            {
                if (mSocket != null && mSocket.Connected && mSocket.Poll(0, SelectMode.SelectWrite))
                {
                    mSendBuffer.Flush();
                }
            }

            private void ProcessExceptions()
            {
                if (mSocket != null && mSocket.Connected && mSocket.Poll(0, SelectMode.SelectError))
                {
                    object error = mSocket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Error);
                    OnError(error as string);
                    mSocket.Close();
                }
            }

            private void ProcessCommand()
            {
                if (mSocket != null && mSocket.Connected)
                {
                    for (int i = 0; i < mMaxMsgPerFrame; i++)
                    {
                        //消息头   
                        if (!mRecvBuffer.Peek(mPackageHeader, MSG_HEADER_LEN))
                            break;      
                        int tid = mPackageHeader[0];
                        int gid = mPackageHeader[5];
                        int uid = mPackageHeader[6];
                        int size = convert(mPackageHeader, 1);
                        //消息体
                        if (mRecvBuffer.Length() < MSG_HEADER_LEN + size)
                            break;                      
                        mRecvBuffer.Skip(MSG_HEADER_LEN);
                        mRecvBuffer.Read(mPackageData, size);
                        //处理网络消息
                        OnMessage(tid, gid, uid, size);
                    }
                }
            }

            private int convert(byte[] buffer, int offset)
            {
                //大端模式
                byte a = buffer[offset];
                byte b = buffer[offset + 1];
                byte c = buffer[offset + 2];
                byte d = buffer[offset + 3];
                return (a << 24) | (b << 16) | (c << 8) | d; 
            }

            private void convert(byte[] buffer, int offset, int size)
            {
                buffer[offset] = (byte)(size >> 24);
                buffer[offset + 1] = (byte)((size & 0x00FF0000) >> 16);
                buffer[offset + 2] = (byte)((size & 0x0000FF00) >> 8);
                buffer[offset + 3] = (byte)((size & 0x000000FF));
            }

            private void OnError(string error)
            {
                mLuaFunction.BeginPCall();
                mLuaFunction.Push((int)NET_EVENT.ERROR);
                mLuaFunction.Push(mSocketIndex);
                mLuaFunction.Push(error);
                mLuaFunction.PCall();
                mLuaFunction.EndPCall();
            }

            private void OnMessage(int tid, int gid, int uid, int size)
            {
                mLuaFunction.BeginPCall();
                mLuaFunction.Push((int)NET_EVENT.MESSAGE);
                mLuaFunction.Push(mSocketIndex);
                mLuaFunction.Push(tid);
                mLuaFunction.Push(gid);
                mLuaFunction.Push(uid);
                mLuaFunction.Push(new LuaByteBuffer(mPackageData, size));
                mLuaFunction.PCall();
                mLuaFunction.EndPCall();
            }

            private void OnConnect()
            {
                mLuaFunction.BeginPCall();
                mLuaFunction.Push(mSocket.Connected ? (int)NET_EVENT.CONNECT : (int)NET_EVENT.DISCONNECT);
                mLuaFunction.Push(mSocketIndex);
                mLuaFunction.PCall();
                mLuaFunction.EndPCall();
            }
        }
        public class HttpObject
        {
            public LuaFunction mLuaFunction;
            public int mCallBackIndex = 0;
            public Dictionary<int, UnityWebRequestAsyncOperation> mRequestDic = new Dictionary<int, UnityWebRequestAsyncOperation>(8);
            public List<int> mRequestDone = new List<int>(8);

            public HttpObject(LuaFunction luaFunc)
            {
                mLuaFunction = luaFunc;
            }

            public void Update()
            {
                mRequestDone.Clear();
                foreach (var item in mRequestDic)
                {
                    if(item.Value.webRequest.isDone || item.Value.webRequest.isNetworkError)
                    {
                        mRequestDone.Add(item.Key);
                    }
                }
                for (int i = 0; i < mRequestDone.Count; i++)
                {
                    int callKey = mRequestDone[i];
                    UnityWebRequestAsyncOperation operation = mRequestDic[callKey];
                    mRequestDic.Remove(callKey);

                    mLuaFunction.BeginPCall();
                    mLuaFunction.Push(callKey);
                    mLuaFunction.Push(operation.webRequest.isNetworkError);
                    mLuaFunction.Push(operation.webRequest.error);
                    mLuaFunction.Push(operation.webRequest.downloadHandler.text);
                    mLuaFunction.PCall();
                    mLuaFunction.EndPCall();
                }
                mRequestDone.Clear();
            }

            public int HttpSend(string url)
            {
                mCallBackIndex++;
                UnityWebRequest request = UnityWebRequest.Get(url);
                mRequestDic[mCallBackIndex] = request.SendWebRequest();
                return mCallBackIndex;
            }
        }

        private bool mInit = false;
        private List<SocketObject> mSocketObjects = new List<SocketObject>(8);
        private HttpObject mHttpObject;

        public void Init(int socketCount, LuaFunction socketEvent,LuaFunction httpEvent, int maxMsgPerFrame)
        {
            if (!mInit)
            {
                mInit = true;
                for (int i = 0; i < socketCount; i++)
                {
                    mSocketObjects.Add(new SocketObject(i, socketEvent, maxMsgPerFrame));
                }
                mHttpObject = new HttpObject(httpEvent);
            }
        }

        public override void Update()
        {
            for (int i = 0; i < mSocketObjects.Count; i++)
            {
                mSocketObjects[i].Update();
            }
            if(mHttpObject != null)
                mHttpObject.Update();
        }

        public override void Exit()
        {
            base.Exit();
            for (int i = 0; i < mSocketObjects.Count; i++)
            {
                mSocketObjects[i].Close();
            }
        }

        public void SocketConnect(int socketIndex,string ip,int port)
        {
            if(socketIndex >= mSocketObjects.Count)
            {
                Debug.LogError(string.Format("Socket {0} 尚未注册!", socketIndex));
                return;
            }
            mSocketObjects[socketIndex].Connect(ip, port);
        }

        public void SocketClose(int socketIndex)
        {
            if (socketIndex >= mSocketObjects.Count)
            {
                Debug.LogError(string.Format("Socket {0} 尚未注册!", socketIndex));
                return;
            }
            mSocketObjects[socketIndex].Close();
        }

        public void SocketSend(int socketIndex, int tid, int gid, int uid, byte[] data)
        {
            if (socketIndex >= mSocketObjects.Count)
            {
                Debug.LogError(string.Format("Socket {0} 尚未注册!", socketIndex));
                return;
            }
            mSocketObjects[socketIndex].Send(tid, gid, uid, data);
        }

        public int HttpSend(string url)
        {
            return mHttpObject.HttpSend(url);
        }
    }
}
