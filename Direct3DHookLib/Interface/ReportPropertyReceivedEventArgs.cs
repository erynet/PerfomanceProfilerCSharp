using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Direct3DHookLib.Interface
{
    [Serializable]   
    public class ReportPropertyReceivedEventArgs: MarshalByRefObject
    {
        //public MessageType MessageType { get; set; }
        //public string Message { get; set; }
        //public System.Collections.IDictionary Property { get; set; }
        public Int32 Pid { get; set; }
        public float DxVersion { get; set; }

        public ReportPropertyReceivedEventArgs(Int32 pid, float dxVersion)
        {
            System.Console.WriteLine(" #####ReportPropertyReceivedEventArgs / pid : " + pid + ", dxVersion : " + dxVersion);
            Pid = pid;
            DxVersion = dxVersion;
        }

        

        public override string ToString()
        {
            return String.Format("Pid : {0}, DirectX Version {1}", Pid, DxVersion);
        }
    }
}