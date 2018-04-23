using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System;
using GameCore;
using System.IO;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.SceneManagement;

namespace GameLogic
{
    public class GameTest : MonoBehaviour
    {

        private void Awake()
        {
            
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {

        }

        private void Update()
        {
            transform.Rotate(Vector3.up, 10 * Time.deltaTime, Space.Self);
        }
    }
}
