using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace GameCore
{
    public sealed class UIBehaviour : MonoBehaviour
    {
        private void Awake()
        {
            
        }

        private void Start()
        {
            
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }
    } 

    public static class UIMgr
    {
        private static Dictionary<int, UIBehaviour> mUIDic;

        public static void Init()
        {
            ExposedReference<MonoBehaviour> tt;
            Playable p;
            PlayableBehaviour pp;
            PlayableDirector pd;
        }

        public static void Exit()
        {

        }

        public static void Register()
        {

        }

        public static void Open(int id)
        {

        }

        public static void Close(int id)
        {

        }
    }
}
