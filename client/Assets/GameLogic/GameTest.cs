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
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.LoadScene("0",LoadSceneMode.Additive);
            SceneManager.LoadScene("1", LoadSceneMode.Additive);
        }

        private void OnDisable()
        {
            SceneManager.UnloadSceneAsync("0");
            Scene scene = SceneManager.GetSceneByName("0");
            if (scene.IsValid())
            {
                int x = 0;
            }
        }
    }
}
