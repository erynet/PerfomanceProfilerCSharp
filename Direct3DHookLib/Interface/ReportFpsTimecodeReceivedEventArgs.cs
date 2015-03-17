using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Direct3DHookLib.Interface
{
    [Serializable]   
    public class ReportFpsTimecodeReceivedEventArgs: MarshalByRefObject
    {
        //public float Fps { get; set; }
        public Int64 Ticks { get; set; }

        //DateTime.Now.Ticks g;

        public ReportFpsTimecodeReceivedEventArgs(Int64 ticks)
        {
            Ticks = ticks;
            //System.Console.WriteLine(" #####ReportFpsReceivedEventArgs / fps : " + Fps);
        }

        public override string ToString()
        {
            return String.Format("Ticks : {0}", Ticks);
        }
    }
}