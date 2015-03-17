using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Direct3DHookLib.Interface;

namespace Direct3DHookLib.Hook
{
    internal interface IDXHook: IDisposable
    {
        CaptureInterface Interface
        {
            get;
            set;
        }
        CaptureConfig Config
        {
            get;
            set;
        }

        ScreenshotRequest Request
        {
            get;
            set;
        }

        void Hook();

        void Cleanup();
    }
}
