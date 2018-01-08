using com.cyou.chat;
using com.cyou.chat.client;
using com.cyou.chat.net;
using com.cyou.media.cos;
using com.cyou.chat.translator;
using com.cyou.media.speech;

namespace ldj.sdk.cyou.chat
{
    public static class GameChatServer
    {
        private static IClient mServer;
        private static int mServerStatus = 0; //0未连接  1连接中 2登录中 3已连接

        //网络连接初始化参数
        private static string IP = "211.159.180.192";
        private static int PORT = 7272;
        private static string USER_TOKEN = "";
        private static string USER_ID = "123";
        private static string USER_NAME = "456";
        private static string PROJ_NAME = "com.cyou.ldj";

        public static void InitServer()
        {
            if (mServerStatus == 0)
            {
                //创建SOCKET客户端
                mServerStatus = 1;
                mServer = ChatSystem.Instance.CreateClient(IP, PORT, OnClientConnect, OnClientDisConnect, new ServerMsgHandler());
            }
        }

        public static int SendChatMsg(string chat_info, string players)
        {
            if (mServerStatus == 3)
            {
                Web_CT_Say_PlayerList msg = new Web_CT_Say_PlayerList();
                msg.chat_info = chat_info;
                msg.player_list = players.Split(',');
                mServer.SendPacket(msg);
                return -1;
            }
            else
            {
                return mServerStatus;
            }
        }

        private static void OnClientConnect(bool success, string result)
        {
            if (success)
            {
                mServerStatus = 2;
                Web_CT_ServerConnect msg = new Web_CT_ServerConnect();
                msg.verify_code = "vv11";
                msg.salt = "11";
                mServer.SendPacket(msg);
            }
            else
            {
                mServerStatus = 0;
                GameCore.LogMgr.LogError(result);
            }
        }

        private static void OnClientDisConnect()
        {
            mServerStatus = 0;
        }

        private class ServerMsgHandler : Web_MessageHandler
        {
            public override void HandleWebPacket(Web_TC_ServerConnect msg)
            {
                Web_CT_ServerPlayerLogin msgLogin = new Web_CT_ServerPlayerLogin();

                msgLogin.accid = USER_ID;
                mServer.SendPacket(msgLogin);
            }

            public override void HandleWebPacket(Web_TC_ServerPlayerLogin msg)
            {
                ChatManager.Instance.InitClientParam(IP,PORT, USER_ID, USER_NAME,PROJ_NAME,msg.token);
                ChatManager.Instance.InitClient();
            }
        }
    }
}
