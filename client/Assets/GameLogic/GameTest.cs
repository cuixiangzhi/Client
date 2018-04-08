using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System;
using GameCore;
using System.IO;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GameLogic
{
    public class GameTest : MonoBehaviour
    {
        public Material mo;

        public Material mi1;

        public Material mi2;

        private void Awake()
        {
            mo = Resources.Load<Material>("tt");                   
        }

        private void OnEnable()
        {
            mi1 = mo;
            mi2 = mo;
            mi1.mainTexture = null;
            Resources.UnloadAsset(mi1);
        }
    }
}
