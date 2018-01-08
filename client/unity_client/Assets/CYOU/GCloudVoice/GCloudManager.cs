using UnityEngine;
using gcloud_voice;

namespace com.tencent.gcloud
{
    public class GCloudManager : MonoBehaviour
    {
        private IGCloudVoice mGCloudEngine = null;

        private string mRoomName;
        private int mRoomStatus = 0; //0空闲 1进入中 2房间内 3退出中
        private bool mMicOpen = false;
        private bool mSpeakerOpen = false;

        private static GCloudManager mInstance;
        public static GCloudManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    GameObject go = new GameObject("RealtimeVoice_Tencent");
                    mInstance = go.AddComponent<GCloudManager>();
                    DontDestroyOnLoad(go);
                }
                return mInstance;
            }
        }

        public void Init(string appID,string appKey,string playerID,string serverURL)
        {
            mGCloudEngine = GCloudVoice.GetEngine();
            mGCloudEngine.SetAppInfo(appID, appKey, playerID);
            mGCloudEngine.SetServerInfo(serverURL);
            mGCloudEngine.Init();           

            mGCloudEngine.OnJoinRoomComplete += (IGCloudVoice.GCloudVoiceCompleteCode code, string roomName, int memberID) =>
            {
                mRoomName = roomName;
                mRoomStatus = 2;
                if (mMicOpen)
                    mGCloudEngine.OpenMic();
                else
                    mGCloudEngine.CloseMic();
                if (mSpeakerOpen)
                    mGCloudEngine.OpenSpeaker();
                else
                    mGCloudEngine.CloseSpeaker();
            };
            mGCloudEngine.OnQuitRoomComplete += (IGCloudVoice.GCloudVoiceCompleteCode code, string roomName, int memberID) =>
            {
                mRoomName = null;
                mRoomStatus = 0;
            };

            mGCloudEngine.OnMemberVoice += (int[] members, int count) =>
            {
                
            };
            mGCloudEngine.SetMode(GCloudVoiceMode.RealTime);
        }

        public int JoinTeamRoom(string roomName,int timeout = 15000)
        {                      
            if (mGCloudEngine != null && mRoomStatus == 0)
            {
                mRoomStatus = 1;
                mGCloudEngine.JoinTeamRoom(roomName, timeout);
            }
            else
            {
                return mRoomStatus;
            }
            return -1;
        }

        public int JoinNationalRoom(string roomName, bool canSpeak, int timeout = 15000)
        {           
            if (mGCloudEngine != null && mRoomStatus == 0)
            {
                mRoomStatus = 1;
                mGCloudEngine.JoinNationalRoom(roomName, canSpeak ? GCloudVoiceRole.ANCHOR : GCloudVoiceRole.AUDIENCE, timeout);
            }
            else
            {
                return mRoomStatus;
            }
            return -1;
        }

        public int QuitRoom(int timeout = 15000)
        {         
            if (mGCloudEngine != null && mRoomStatus == 2 && mRoomName != null)
            {
                mRoomStatus = 3;
                mGCloudEngine.QuitRoom(mRoomName, timeout);
            }
            else
            {
                return mRoomStatus;
            }
            return -1;
        }
        public void OpenMic()
        {
            mMicOpen = true;
            if (mGCloudEngine != null && mRoomStatus == 2)
            {
                mGCloudEngine.OpenMic();
            }
        }
        public void CloseMic()
        {
            mMicOpen = false;
            if (mGCloudEngine != null && mRoomStatus == 2)
            {
                mGCloudEngine.CloseMic();
            }
        }
        public void OpenSpeaker()
        {
            mSpeakerOpen = true;
            if (mGCloudEngine != null && mRoomStatus == 2)
            {
                mGCloudEngine.OpenSpeaker();
            }
        }
        public void CloseSpeaker()
        {
            mSpeakerOpen = false;
            if (mGCloudEngine != null && mRoomStatus == 2)
            {
                mGCloudEngine.CloseSpeaker();
            }           
        }

        void Update()
        {
            if (mGCloudEngine != null && mRoomStatus != 0)
            {
                mGCloudEngine.Poll();
            }
        }
        public void OnApplicationPause(bool pauseStatus)
        {
            if(mGCloudEngine != null && mRoomStatus != 0)
            {
                if(pauseStatus)
                {
                    mGCloudEngine.Pause();
                }
                else
                {
                    mGCloudEngine.Resume();
                }
            }
        }
    }
}
