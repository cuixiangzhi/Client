using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrameWork
{
    public interface ThreadBase
    {
        void Init();
        void Loop();
        void Exit();
        bool CanRunInThread();
    }
}
