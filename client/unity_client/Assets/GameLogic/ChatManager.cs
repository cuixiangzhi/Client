using com.cyou.chat;
using com.cyou.chat.client;
using com.cyou.chat.net;
using com.cyou.media.cos;
using com.cyou.chat.translator;
using com.cyou.media.speech;
using com.tencent.gcloud;
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using LuaInterface;

namespace ldj.sdk.cyou.chat
{
    public enum CHAT_EVENT_TYPE
    {
        AUDIO_BREAK = 0,
        AUDIO_INVALID1 = 1,
        AUDIO_CANCEL = 2,
        AUDIO_TOO_SHORT = 3,
        AUDIO_INVALID2 = 4,
        AUDIO_SAVE_FAIL = 5,
        AUDIO_UPLOAD = 6,
        AUDIO_UPLOAD_FAIL = 7,
        AUDIO_DOWN_FAIL = 8,
        AUDIO_PLAY_FINISH = 9,
        CHAT_MSG_NEW = 10,
        REALTIME_NATIONAL_FAIL = 11,
        REALTIME_TEAM_FAIL = 12,
    }

    public class ChatManager : MonoBehaviour
    {
        private static ChatManager mInstance;
        public static ChatManager Instance
        {
            get
            {
                if(mInstance == null)
                {
                    GameObject go = new GameObject("ChatManager");
                    mInstance = go.AddComponent<ChatManager>();
                    DontDestroyOnLoad(go);
                }
                return mInstance;
            }
        }

        private IClient mClient;
        private int mClientStatus = 0; //0未连接  1连接中 2登录中 3已连接

        private string mSpeechContent;
        private DateTime mSpeechBeginTime;
        private int mTotalSeconds;
        private bool mNeedUploadVoice;
        private int mSpeechStatus = 0; //0空闲 1录音中 2取消中 3识别中 4上传中 5下载中 6播放中 7实时模式

        private string mFinalPlayFile;

        private LuaFunction LUA_EVENT;

        #region SDK初始化
        //网络连接初始化参数
        private string IP = "211.159.180.192";
        private int PORT = 7272;
        private string USER_TOKEN = "";
        private string USER_ID = "1234";
        private string USER_NAME = "";
        private string PROJ_NAME = "com.cyou.ldj";

        //搜狗语音初始化参数
        private string SG_APPID = "RDKO602";
        private string SG_APPKEY = "zxb8w4q5";
        private string SG_PATH = "";
        
        //百度翻译初始化参数
        private string BAIDU_APPID = "20160524000021917";
        private string BAIDU_APPKEY = "rVK7DZap0nwSS5XVCEtA";

        //文件存储初始化参数
        private string COS_APPID = "1255801262";
        private string COS_BUCKET_NAME = "ldj";
        private string COS_SECRET_ID = "AKID9FD3mJbqqibbVHrJZeg2FJCvY4Y4NZXJ";
        private string COS_SECRET_KEY = "mjYFl7GGmJtNkw1v1xqcHk3FHJBvTp9K";
        private string COS_REGION = "bj";
        private bool COS_DEBUG = true;
        private string COS_DIR = "";

        //实时语音初始化参数
        private string GCLOUD_APPID = "932849489";
        private string GCLOUD_APPKEY = "d94749efe9fce61333121de84123ef9b";
        private string GCLOUD_APPURL = "";

        public void InitEventFunc(LuaFunction func)
        {
            LUA_EVENT = func;
        }

        public void InitClientParam(string ip, int port, string userID, string userName, string projName, string userToken)
        {
            IP = ip;
            PORT = port;
            USER_ID = userID;
            USER_NAME = userName;
            USER_TOKEN = userToken;
            PROJ_NAME = projName;
        }

        public void InitSpeechParam(string appID,string appKey)
        {
            SG_APPID = appID;
            SG_APPKEY = appKey;
        }

        public void InitTranslateParam(string appID,string appKey)
        {
            BAIDU_APPID = appID;
            BAIDU_APPKEY = appKey;
        }

        public void InitCosParam(string appID, string bucketName, string secretID, string secretKey, string region, bool debug,string cosDir)
        {
            COS_APPID = appID;
            COS_BUCKET_NAME = bucketName;
            COS_SECRET_ID = secretID;
            COS_SECRET_KEY = secretKey;
            COS_REGION = region;
            COS_DEBUG = debug;
            COS_DIR = cosDir;
        }

        public void InitGCloudParam(string appID,string appKey,string appURL)
        {
            GCLOUD_APPID = appID;
            GCLOUD_APPKEY = appKey;
            GCLOUD_APPURL = appURL;
        }

        public void InitSDK(bool debug)
        {
#if UNITY_EDITOR
            SG_PATH = Application.dataPath + "/../Record";
#else
            SG_PATH = Application.persistentDataPath + "/Record";
#endif
            if (!Directory.Exists(SG_PATH))
            {
                Directory.CreateDirectory(SG_PATH);
            }
            ChatSystem.Instance.SetDebug(debug);
            //输入历史初始化
            Recommend.Instance.InitFromDisk();
            //搜狗语音初始化
            SpeechManager.Instance.Init(SG_APPID, SG_APPKEY);                  

            SpeechManager.Instance.SetRecordBeginHandler(OnRecordBegin);
            SpeechManager.Instance.SetRecordEndHandler(OnRecordEnd);

            SpeechManager.Instance.SetTaskStartHandler(OnTaskStart);
            SpeechManager.Instance.SetTaskCancleHandler(OnTaskCancle);
            SpeechManager.Instance.SetTaskOverHandler(OnTaskOver);

            SpeechManager.Instance.SetAudioPlayOverHandler(OnPlayRecordOver);

            SpeechManager.Instance.SetTextProcessHandler(OnReceiveSpeechText);
            SpeechManager.Instance.SetErrorProcessHandler(OnReceiveErrorCode);
            SpeechManager.Instance.SetValidVoiceDetectedHandler(OnReceiveValidVoice);
            SpeechManager.Instance.SetHasPermissionHandler(OnReceiveHasPermission);
            //文件存储初始化
            TXCosManager.Instance.InitCos(COS_APPID, COS_BUCKET_NAME, COS_SECRET_ID, COS_SECRET_KEY, COS_REGION, debug);
            TXCosManager.Instance.OnUploadFileAction = OnUploadFile;
            TXCosManager.Instance.OnDownloadAction = OnDownloadFile;
            //百度翻译初始化
            TranslateEngine.Instance.SetTranslateBaiduKey(BAIDU_APPID, BAIDU_APPKEY);
            TranslateEngine.Instance.InitTranslator(TranslatorType.TT_Baidu);
            //实时语音初始化
            GCloudManager.Instance.Init(GCLOUD_APPID,GCLOUD_APPKEY,USER_ID, GCLOUD_APPURL, OnJoinRoomFail);
        }

        public void InitClient()
        {
            if(mClientStatus == 0)
            {
                //创建SOCKET客户端
                mClientStatus = 1;
                mClient = ChatSystem.Instance.CreateClient(IP, PORT, OnClientConnect, OnClientDisConnect, new ClientMsgHandler());
            }
        }       

        public void Update()
        {
            ChatSystem.Instance.Tick();
        }

        public void OnApplicationQuit()
        {
            Recommend.Instance.SaveToDisk();
            mInstance = null;
        }
        #endregion

        #region 搜狗语音

        public int StartRecord()
        {
            if (mSpeechStatus == 0 || mSpeechStatus == 5 || mSpeechStatus == 6)
            {
                if (mSpeechStatus == 5 || mSpeechStatus == 6)
                {
                    mFinalPlayFile = string.Empty;
                    SendEvent(CHAT_EVENT_TYPE.AUDIO_BREAK);
                }
                mSpeechStatus = 1;
                SpeechManager.Instance.StartRecording();
                return -1;
            }
            else
            {
                return mSpeechStatus;
            }
        }

        public int CancelRecord()
        {
            if(mSpeechStatus == 1)
            {
                mSpeechStatus = 2;
                mNeedUploadVoice = false;
                SpeechManager.Instance.CancleTask();
                return -1;
            }
            else
            {
                return mSpeechStatus;
            }
        }

        public int StopRecord()
        {
            if(mSpeechStatus == 1)
            {
                mSpeechStatus = 3;
                mNeedUploadVoice = true;
                SpeechManager.Instance.StopRecording();
                return -1;
            }
            else
            {
                return mSpeechStatus;
            }
        }

        public int StartAudio(string remotePath)
        {
            if(mSpeechStatus == 0 || mSpeechStatus == 5 || mSpeechStatus == 6)
            {
                mFinalPlayFile = GetRecordLocalPath(remotePath);
                if (File.Exists(mFinalPlayFile))
                {
                    mSpeechStatus = 6;
                    SpeechManager.Instance.PlayAudio(File.ReadAllBytes(mFinalPlayFile));
                }
                else
                {
                    mSpeechStatus = 5;
                    TXCosManager.Instance.DownloadFile(remotePath, mFinalPlayFile);
                }
                return -1;
            }
            else
            {
                return mSpeechStatus;
            }
        }

        public int StopAudio()
        {
            if (mSpeechStatus == 5 || mSpeechStatus == 6)
            {
                mSpeechStatus = 0;
                mFinalPlayFile = string.Empty;
                SpeechManager.Instance.StopAudio();
                return -1;
            }
            else
            {
                return mSpeechStatus;
            }
        }
        #endregion

        #region 实时语音
        public void OpenMic()
        {
            GCloudManager.Instance.OpenMic();
        }

        public void CloseMic()
        {
            GCloudManager.Instance.CloseMic();
        }

        public void OpenSpeaker()
        {
            GCloudManager.Instance.OpenSpeaker();
        }

        public void CloseSpeaker()
        {
            GCloudManager.Instance.CloseSpeaker();
        }

        public void JoinTeamRoom(string roomName)
        {
            GCloudManager.Instance.JoinTeamRoom(roomName);
        }

        public void JoinNationalRoom(string roomName, bool canSpeak)
        {
            GCloudManager.Instance.JoinNationalRoom(roomName, canSpeak);
        }

        public void QuitRoom()
        {
            GCloudManager.Instance.QuitRoom();
        }
        #endregion

        #region 语音回调
        private void OnReceiveSpeechText(string text)
        {
            //语音识别结果
            mSpeechContent = text;
        }
        
        private void OnReceiveValidVoice(bool isValid)
        {
            
        }
        
        private void OnReceiveHasPermission(bool hasPermission)
        {
            
        }
        
        private void OnReceiveErrorCode(string errorCode)
        {
            if(mSpeechStatus == 1 || mSpeechStatus == 3)
            {
                mSpeechStatus = 0;
                SendEvent(CHAT_EVENT_TYPE.AUDIO_INVALID1);
            }
        }
        
        private void OnTaskStart()
        {
            //通知任务开始
            mSpeechBeginTime = DateTime.Now;
        }
        
        private void OnTaskCancle()
        {
            
        }

        private void OnRecordBegin()
        {

        }

        private void OnRecordEnd()
        {
            mTotalSeconds = (int)Math.Floor((DateTime.Now - mSpeechBeginTime).TotalSeconds);
        }

        private void OnTaskOver()
        {
            if(mSpeechStatus == 3)
            {
                //时间太短
                if (mTotalSeconds <= 0)
                {
                    mSpeechStatus = 0;
                    SendEvent(CHAT_EVENT_TYPE.AUDIO_TOO_SHORT);
                    return;
                }
                //无效语音
                byte[] buffer = SpeechManager.Instance.GetAudioBuffer();
                if (buffer == null)
                {
                    mSpeechStatus = 0;
                    SendEvent(CHAT_EVENT_TYPE.AUDIO_INVALID2);
                    return;
                }
                string localPath = GetRecordLocalPath(string.Empty);
                FileStream file = null;
                bool success = true;
                try
                {
                    //本地保存
                    file = new FileStream(localPath, FileMode.Create);
                    file.Write(buffer, 0, buffer.Length);                                                    
                }
                catch(Exception e)
                {
                    //保存失败
                    GameCore.LogMgr.LogError(e.Message);
                    success = false;
                }
                finally
                {
                    if(file != null)
                    {
                        file.Close();
                    }
                }
                if (success)
                {
                    //上传语音
                    mSpeechStatus = 4;
                    TXCosManager.Instance.UploadFile(localPath, COS_DIR, -1);
                }
                else
                {
                    mSpeechStatus = 0;
                    SendEvent(CHAT_EVENT_TYPE.AUDIO_SAVE_FAIL);
                }
            }
            else if(mSpeechStatus == 2)
            {
                mSpeechStatus = 0;
                SendEvent(CHAT_EVENT_TYPE.AUDIO_CANCEL);
            }
        }
        


        private void OnPlayRecordOver()
        {
            if(mSpeechStatus == 6)
            {
                mSpeechStatus = 0;
                SendEvent(CHAT_EVENT_TYPE.AUDIO_PLAY_FINISH);
            }
        }

        private void OnUploadFile(string code, string localPath, string remotePath)
        {
            if(mSpeechStatus == 4)
            {
                mSpeechStatus = 0;
                if (code == "0")
                {                 
                    SendEvent(CHAT_EVENT_TYPE.AUDIO_UPLOAD, remotePath,mSpeechContent);
                }
                else
                {
                    SendEvent(CHAT_EVENT_TYPE.AUDIO_UPLOAD_FAIL, remotePath);
                }
            }
        }

        private void OnDownloadFile(string code, string localPath, string remotePath)
        {
            if (mSpeechStatus == 5)
            {
                if(code == "0")
                {
                    mSpeechStatus = 6;
                    SpeechManager.Instance.PlayAudio(File.ReadAllBytes(localPath));
                }
                else
                {
                    mSpeechStatus = 0;
                    SendEvent(CHAT_EVENT_TYPE.AUDIO_DOWN_FAIL);
                }
            }
        }
        #endregion

        #region 网络消息
        private void OnClientConnect(bool success,string result)
        {
            if(success)
            {
                mClientStatus = 2;
                Web_CT_Login msg = new Web_CT_Login();
                msg.accid = USER_ID;
                msg.proj_name = PROJ_NAME;
                msg.token = USER_TOKEN;
                msg.usr_name = USER_NAME;
                mClient.SendPacket(msg);                
            }
            else
            {
                //通知连接失败
                mClientStatus = 0;
            }
        }

        private void OnClientDisConnect()
        {
            //通知断开连接
            mClientStatus = 0;
        }

        private class ClientMsgHandler : Web_MessageHandler
        {

            public override void HandleWebPacket(Web_TC_Login msg)
            {
                //通知连接成功
                Instance.mClientStatus = 3;
            }

            public override void HandleWebPacket(Web_TC_Say msg)
            {
                //通知聊天消息
                Instance.SendEvent(CHAT_EVENT_TYPE.CHAT_MSG_NEW, msg.chat_info);
            }

            public override void HandleWebPacket(Web_TC_Notify2 msg)
            {
                //通知命令错误
                GameCore.LogMgr.LogError(msg.errcmd);
            }
        }
        #endregion

        public int SendChatMsg(string chat_info, string players)
        {
            if (mClientStatus == 3)
            {
                Web_CT_Say_PlayerList msg = new Web_CT_Say_PlayerList();
                msg.chat_info = chat_info;
                msg.player_list = new List<string>(players.Split(','));
                mClient.SendPacket(msg);
                return -1;
            }
            else
            {
                return mClientStatus;
            }
        }

        private void SendEvent(CHAT_EVENT_TYPE eventID,string arg1 = "",string arg2 = "")
        {
            if(LUA_EVENT != null)
            {
                LUA_EVENT.BeginPCall();
                LUA_EVENT.Push((int)eventID);
                LUA_EVENT.Push(arg1);
                LUA_EVENT.Push(arg2);
                LUA_EVENT.PCall();
                LUA_EVENT.EndPCall();
            }
        }

        private string GetRecordLocalPath(string fileName)
        {
            if(string.IsNullOrEmpty(fileName))
            {
                return string.Format("{0}/Record/{1}_{2}.sound", SG_PATH, USER_ID, DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
            }
            else
            {
                return string.Format("{0}/Record/{1}", SG_PATH, Path.GetFileName(fileName));
            }                     
        }

        private void OnJoinRoomFail(int evtID,int ret)
        {
            if(evtID == 0)
            {
                SendEvent(CHAT_EVENT_TYPE.REALTIME_NATIONAL_FAIL,ret.ToString());
            }
            else
            {
                SendEvent(CHAT_EVENT_TYPE.REALTIME_TEAM_FAIL, ret.ToString());
            }
        }
    }
}
