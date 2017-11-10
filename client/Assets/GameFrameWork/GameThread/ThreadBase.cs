using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GF
{
    public interface ThreadBase
    {
        void Init();
        void Loop();
        void Exit();
        bool CanRunInThread();
    }
}
