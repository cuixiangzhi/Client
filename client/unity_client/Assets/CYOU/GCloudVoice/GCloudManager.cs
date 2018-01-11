using UnityEngine;
using gcloud_voice;
using System;

namespace com.tencent.gcloud
{
    public class GCloudManager : MonoBehaviour
    {
        private IGCloudVoice mGCloudEngine = null;
        private int mTimeOut = 15000;

        private string mFinalRoomName;
        private bool mIsNationalRoom;
        private bool mCanSpeak;
        private string mCurrentRoomName;
        private Action<int, int> mOnJoinRoomFail;

        private string mOpenID;
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

        public void Init(string appID,string appKey,string playerID,string serverURL,Action<int,int> onJoinRoomFail)
        {
            mOnJoinRoomFail = onJoinRoomFail;
            mOpenID = SystemInfo.deviceUniqueIdentifier + playerID;
            mGCloudEngine = GCloudVoice.GetEngine();
            mGCloudEngine.SetAppInfo(appID, appKey, mOpenID);
            mGCloudEngine.SetServerInfo(serverURL);
            mGCloudEngine.Init();           

            mGCloudEngine.OnJoinRoomComplete += (IGCloudVoice.GCloudVoiceCompleteCode code, string roomName, int memberID) =>
            {
                if(mRoomStatus == 1)
                {
                    mCurrentRoomName = roomName;
                    mRoomStatus = 2;
                    if (mMicOpen)
                        mGCloudEngine.OpenMic();
                    else
                        mGCloudEngine.CloseMic();
                    if (mSpeakerOpen)
                        mGCloudEngine.OpenSpeaker();
                    else
                        mGCloudEngine.CloseSpeaker();
                    JoinFinalRoom();
                }
            };
            mGCloudEngine.OnQuitRoomComplete += (IGCloudVoice.GCloudVoiceCompleteCode code, string roomName, int memberID) =>
            {
                if(mRoomStatus == 3)
                {
                    mCurrentRoomName = string.Empty;
                    mRoomStatus = 0;
                    JoinFinalRoom();
                }
            };

            mGCloudEngine.OnMemberVoice += (int[] members, int count) =>
            {
                
            };
            mGCloudEngine.SetMode(GCloudVoiceMode.RealTime);
        }

        private void JoinFinalRoom()
        {
            if(mFinalRoomName != mCurrentRoomName && !string.IsNullOrEmpty(mFinalRoomName))
            {
                if(mIsNationalRoom)
                {
                    JoinNationalRoom(mFinalRoomName, mCanSpeak);
                }
                else
                {
                    JoinTeamRoom(mFinalRoomName);
                }
            }
        }

        public void JoinTeamRoom(string roomName)
        {
            if (mGCloudEngine != null)
            {
                mFinalRoomName = roomName;
                mIsNationalRoom = false;
                if(mRoomStatus == 0)
                {
                    mRoomStatus = 1;
                    int ret = mGCloudEngine.JoinTeamRoom(mFinalRoomName, mTimeOut);
                    if(ret != 0)
                    {
                        OnJoinRoomFail(1, ret);
                    }
                }
                else if(mRoomStatus == 2)
                {
                    mRoomStatus = 3;
                    mGCloudEngine.QuitRoom(mCurrentRoomName, mTimeOut);
                }
            }
        }

        public void JoinNationalRoom(string roomName, bool canSpeak)
        {
            if (mGCloudEngine != null)
            {
                mFinalRoomName = roomName;
                mIsNationalRoom = true;
                mCanSpeak = canSpeak;
                if (mRoomStatus == 0)
                {
                    mRoomStatus = 1;
                    int ret = mGCloudEngine.JoinNationalRoom(mFinalRoomName, mCanSpeak ? GCloudVoiceRole.ANCHOR : GCloudVoiceRole.AUDIENCE, mTimeOut);
                    if(ret != 0)
                    {
                        OnJoinRoomFail(0,ret);
                    }
                }
                else if(mRoomStatus == 2)
                {
                    mRoomStatus = 3;
                    mGCloudEngine.QuitRoom(mCurrentRoomName, mTimeOut);
                }
            }
        }

        public void QuitRoom()
        {
            if (mGCloudEngine != null)
            {
                mFinalRoomName = string.Empty;
                if (mRoomStatus == 2 && !string.IsNullOrEmpty(mCurrentRoomName))
                {
                    mRoomStatus = 3;
                    mGCloudEngine.QuitRoom(mCurrentRoomName, mTimeOut);
                }
            }
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

        private void Update()
        {
            if (mGCloudEngine != null && mRoomStatus != 0)
            {
                mGCloudEngine.Poll();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
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

        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
           if(mGCloudEngine != null)
            {
                QuitRoom();
                mGCloudEngine.Deinit();
            }
#endif
        }

        private void OnJoinRoomFail(int evtID,int ret)
        {
            mRoomStatus = 0;
            if(mOnJoinRoomFail != null)
            {
                mOnJoinRoomFail(evtID, ret);
            }
        }
    }
}
