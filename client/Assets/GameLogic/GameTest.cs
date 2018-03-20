using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System;
using GameCore;
using System.IO;

namespace GameLogic
{
    public class GameTest : MonoBehaviour
    {
        public int byteLen = 0;

        private void Start()
        {
            IntPtr file = UtilDll.common_open(Application.streamingAssetsPath + "/t.bytes", "rb");
            int x = 0;
        }

        private void OnGUI()
        {
            
        }
    }
}
