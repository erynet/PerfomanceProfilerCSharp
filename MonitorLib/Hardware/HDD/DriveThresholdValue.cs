using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
namespace MonitorLib.Hardware.HDD
{

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  internal struct DriveThresholdValue {
    public byte Identifier;
    public byte Threshold;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public byte[] Unknown;
  }

}
