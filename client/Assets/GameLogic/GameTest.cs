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

        private void Awake()
        {

        }

        private void OnEnable()
        {


            Camera cm = Camera.main;
            Matrix4x4 mlocal = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
            Matrix4x4 mparent = Matrix4x4.TRS(transform.parent.localPosition, transform.parent.localRotation, transform.parent.localScale);
            Matrix4x4 mworld = mparent * mlocal;
            Matrix4x4 mreal = transform.localToWorldMatrix;
        }
    }
}
