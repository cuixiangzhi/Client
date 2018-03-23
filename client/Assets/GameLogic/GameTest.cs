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
        public int byteLen = 0;
        public Vector3 pos;

        private void Start()
        {
            pos = Camera.main.WorldToScreenPoint(Vector3.zero);
        }

        private void Update()
        {
            
                
        }
    }
}
