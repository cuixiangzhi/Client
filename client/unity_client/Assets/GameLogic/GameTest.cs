using UnityEngine;
using ldj.sdk.cyou.chat;
using com.cyou.media.cos;
using System.IO;

namespace GameLogic
{
    public class GameTest : MonoBehaviour
    {
        private void Awake()
        {
            FileStream fs = new FileStream(Application.persistentDataPath + "/test_voice.sound",FileMode.Create);
            fs.Write(new byte[100],0,100);
            fs.Close();
            ChatManager.Instance.InitSDK();
            GameChatServer.InitServer();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("进入房间", GUILayout.Width(400), GUILayout.Height(100)))
            {
                ChatManager.Instance.JoinTeamRoom("hello");
                ChatManager.Instance.OpenMic();
                ChatManager.Instance.OpenSpeaker();
            }
            if (GUILayout.Button("离开房间", GUILayout.Width(400), GUILayout.Height(100)))
            {
                ChatManager.Instance.CloseMic();
                ChatManager.Instance.CloseSpeaker();
                ChatManager.Instance.QuitRoom();
            }
            if (GUILayout.Button("开始录音", GUILayout.Width(400), GUILayout.Height(100)))
            {
                int ret = ChatManager.Instance.StartRecord();
                if(ret != -1)
                {
                    if(ret == 1)
                    {
                        GameCore.LogMgr.LogError("正在录音");
                    }
                    else if(ret == 2)
                    {

                    }
                }
            }
            if (GUILayout.Button("停止录音", GUILayout.Width(400), GUILayout.Height(100)))
            {
                int ret = ChatManager.Instance.StopRecord();
                if(ret != -1)
                {
                    if(ret == 0)
                    {
                        GameCore.LogMgr.LogError("未检测到有效语音");
                    }
                }
            }
            if (GUILayout.Button("上传文件", GUILayout.Width(400), GUILayout.Height(100)))
            {
                TXCosManager.Instance.UploadFile(Application.persistentDataPath + "/test_voice.sound", "ldj", -1);
            }
        }
    }
}
