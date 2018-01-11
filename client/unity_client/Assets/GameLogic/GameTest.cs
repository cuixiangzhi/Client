using UnityEngine;
using ldj.sdk.cyou.chat;
using com.cyou.media.cos;
using System.IO;
using com.cyou.media.speech;
using com.tencent.gcloud;

namespace GameLogic
{
    public class GameTest : MonoBehaviour
    {
        private string SG_APPID = "RDKO602";
        private string SG_APPKEY = "zxb8w4q5";
        private string SG_PATH;

        private string GCLOUD_APPID = "932849489";
        private string GCLOUD_APPKEY = "d94749efe9fce61333121de84123ef9b";
        private string GCLOUD_APPURL = "";

        private void Awake()
        {
            GCloudManager.Instance.Init(GCLOUD_APPID, GCLOUD_APPKEY, SG_APPID, GCLOUD_APPURL, OnJoinRoomFail);
        }

        private void OnGUI()
        {
            if (GUILayout.Button("进入房间", GUILayout.Width(400), GUILayout.Height(100)))
            {
                GCloudManager.Instance.JoinTeamRoom("hello");
                GCloudManager.Instance.OpenMic();
                GCloudManager.Instance.OpenSpeaker();
            }
            if (GUILayout.Button("离开房间", GUILayout.Width(400), GUILayout.Height(100)))
            {
                GCloudManager.Instance.QuitRoom();
            }
        }

        private void OnJoinRoomFail(int evtID,int ret)
        {
            GameCore.LogMgr.LogError("{0} {1}",evtID,ret);
        }
    }
}
