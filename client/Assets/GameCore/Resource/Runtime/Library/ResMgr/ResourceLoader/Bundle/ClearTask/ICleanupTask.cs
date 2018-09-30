using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.cyou.plugin.resource.loader.bundle
{
    interface ICleanupTask
    {
        void SetResourceLoader(ResourceLoader resourceLoader);
        bool TickCleanup(uint uDeltaTimeMS);
        void Cleanup();
        void ForceCleanup();
        void Release();
    }
}
