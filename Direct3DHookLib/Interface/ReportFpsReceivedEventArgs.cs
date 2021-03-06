﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Direct3DHookLib.Interface
{
    [Serializable]   
    public class ReportFpsReceivedEventArgs: MarshalByRefObject
    {
        public double Fps { get; set; }

        public ReportFpsReceivedEventArgs(double fps)
        {   
            Fps = fps;
            //System.Console.WriteLine(" #####ReportFpsReceivedEventArgs / fps : " + Fps);
        }

        public override string ToString()
        {
            return String.Format("Fps : {0}", Fps);
        }
    }
}