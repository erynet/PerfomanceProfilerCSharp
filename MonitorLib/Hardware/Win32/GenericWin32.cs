using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MonitorLib.Hardware.Win32 {
    internal class GenericWin32 : Hardware {

        string processImageName;

        //NativeMethods.OSVersionInfo OsVersionInfo;
        IntPtr hProcess;
        IntPtr hProcessSnap;
        UInt32 pid;
        bool processFound;
        Int32 iterationCount;

        System.Runtime.InteropServices.ComTypes.FILETIME ftSysIdle, ftSysKernel, ftSysUser;
        System.Runtime.InteropServices.ComTypes.FILETIME ftProcCreation, ftProcExit, ftProcKernel, ftProcUser;
        
        System.Runtime.InteropServices.ComTypes.FILETIME prevSysIdle, prevSysKernel, prevSysUser;
        System.Runtime.InteropServices.ComTypes.FILETIME prevProcKernel, prevProcUser;

        //process io counter
        UInt64 prevReadOperationCount;
        UInt64 prevWriteOperationCount;
        UInt64 prevOtherOperationCount;
        UInt64 prevReadTransferCount;
        UInt64 prevWriteTransferCount;
        UInt64 prevOtherTransferCount;

        UInt64 lastRun;

        NativeMethods.PROCESS_MEMORY_COUNTERS pmc;
        NativeMethods.IO_COUNTERS ic;

        private Sensor processCpuUsageSensor;
        private Sensor globalCpuUsageSensor;

        private Sensor workingSetSize;
        private Sensor pagefileUsage;

        private Sensor readOperationCount;
        private Sensor writeOperationCount;
        private Sensor otherOperationCount;
        //private Sensor readTransferCount;
        //private Sensor writeTransferCount;
        //private Sensor otherTransferCount;

        public GenericWin32(string processImageName, ISettings settings) : base("Win32", new Identifier("win32"), settings)
        {
            this.processImageName = processImageName;
            if (AttachProcess(processImageName))
            {
                if (!NativeMethods.GetSystemTimes(out ftSysIdle, out ftSysKernel, out ftSysUser))
                {
                    Log("NativeMethods.GetSystemTimes / GetLastError : " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                    return;
                }

                prevSysIdle = ftSysIdle;
                prevSysKernel = ftSysKernel;
                prevSysUser = ftSysUser;
                
                if (!NativeMethods.GetProcessTimes(hProcess, out ftProcCreation, out ftProcExit, out ftProcKernel, out ftProcUser))
                {
                    Log("NativeMethods.GetProcessTimes / GetLastError : " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                    return;
                }

                prevProcKernel = ftProcKernel;
                prevProcUser = ftProcUser;

                if (!NativeMethods.GetProcessIoCounters(hProcess, out ic))
                {
                    Log("NativeMethods.GetProcessIoCounters / GetLastError : " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                    return;
                }

                prevReadOperationCount = ic.ReadOperationCount;
                prevWriteOperationCount = ic.WriteOperationCount;
                prevOtherOperationCount = ic.OtherOperationCount;
                prevReadTransferCount = ic.ReadTransferCount;
                prevWriteTransferCount = ic.WriteTransferCount;
                prevOtherTransferCount = ic.OtherTransferCount;

                if (!NativeMethods.GetProcessMemoryInfo(hProcess, out pmc, checked((UInt32)Marshal.SizeOf(typeof(NativeMethods.PROCESS_MEMORY_COUNTERS)))))
                {
                    Log("NativeMethods.GetProcessMemoryInfo / GetLastError : " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                    return;
                }

                lastRun = NativeMethods.GetTickCount64();
                //Log("NativeMethods.GetTickCount64 / lastRun : " + lastRun);


                //Create Sensors

                processCpuUsageSensor = new Sensor("CPU Usage(Process)", 0, SensorType.Load, this, settings);
                ActivateSensor(processCpuUsageSensor);
                globalCpuUsageSensor = new Sensor("CPU Usage(Global)", 1, SensorType.Load, this, settings);
                ActivateSensor(globalCpuUsageSensor);

                workingSetSize = new Sensor("Working Set", 0, SensorType.Data2, this, settings);
                ActivateSensor(workingSetSize);
                pagefileUsage = new Sensor("Page file Usage", 1, SensorType.Data2, this, settings);
                ActivateSensor(pagefileUsage);

                readOperationCount = new Sensor("Read Ops", 0, SensorType.IOPS, this, settings);
                ActivateSensor(readOperationCount);
                writeOperationCount = new Sensor("Write Ops", 1, SensorType.IOPS, this, settings);
                ActivateSensor(writeOperationCount);
                otherOperationCount = new Sensor("Other Ops", 2, SensorType.IOPS, this, settings);
                ActivateSensor(otherOperationCount);
                //readTransferCount = new Sensor("Read Transfer", 3, SensorType.IOPS, this, settings);
                //ActivateSensor(readTransferCount);
                //writeTransferCount = new Sensor("Write Transfer", 4, SensorType.IOPS, this, settings);
                //ActivateSensor(writeTransferCount);
                //otherTransferCount = new Sensor("Other Transfer", 5, SensorType.IOPS, this, settings);
                //ActivateSensor(otherTransferCount);
            }
            else
            {
                
            }
        }
        public override HardwareType HardwareType
        {
            get
            {
                return HardwareType.Win32;
            }
        }

        public override void Update()
        {
            //NativeMethods.MemoryStatusEx status = new NativeMethods.MemoryStatusEx();
            //status.Length = checked((uint)Marshal.SizeOf(typeof(NativeMethods.MemoryStatusEx)));

            //if (!NativeMethods.GlobalMemoryStatusEx(ref status))
            //    return;

            //loadSensor.Value = 100.0f - (100.0f * status.AvailablePhysicalMemory) / status.TotalPhysicalMemory;

            //usedMemory.Value = (float)(status.TotalPhysicalMemory - status.AvailablePhysicalMemory) / (1024 * 1024 * 1024);

            //availableMemory.Value = (float)status.AvailablePhysicalMemory / (1024 * 1024 * 1024);


            if (processFound)
            {
                if (!NativeMethods.GetSystemTimes(out ftSysIdle, out ftSysKernel, out ftSysUser))
                {
                    Log("Update() : NativeMethods.GetSystemTimes / GetLastError : " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                    return;
                }
                if (!NativeMethods.GetProcessTimes(hProcess, out ftProcCreation, out ftProcExit, out ftProcKernel, out ftProcUser))
                {
                    Log("Update() : NativeMethods.GetProcessTimes / GetLastError : " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                    return;
                }

UInt64 ftSysKernelDiff = SubtractTimes(ref ftSysKernel, ref prevSysKernel);
	            UInt64 ftSysUserDiff = SubtractTimes(ref ftSysUser, ref prevSysUser);
	            UInt64 ftSysIdleDiff = SubtractTimes(ref ftSysIdle, ref prevSysIdle);
	            UInt64 kernelTotal = (UInt64)(ftSysKernelDiff - ftSysIdleDiff);

                UInt64 ftProcKernelDiff = SubtractTimes(ref ftProcKernel, ref prevProcKernel);
                UInt64 ftProcUserDiff = SubtractTimes(ref ftProcUser, ref prevProcUser);
                UInt64 nTotalSys = ftSysKernelDiff + ftSysUserDiff;
                UInt64 nTotalProc = ftProcKernelDiff + ftProcUserDiff;

                if (nTotalSys > 0){
		            //this->cpuUsage = (float)((100.0 * nTotalProc) / nTotalSys);
		            processCpuUsageSensor.Value = (float)((nTotalProc * 100.0) / nTotalSys);
		            globalCpuUsageSensor.Value = (float)(((kernelTotal + ftSysUserDiff) * 100.0) / nTotalSys);
	            }
	            else {
		            //this->cpuUsage = 0.;
                    processCpuUsageSensor.Value = (float)0.0;
                    globalCpuUsageSensor.Value = (float)0.0;
	            }

                prevSysIdle = ftSysIdle;
                prevSysKernel = ftSysKernel;
                prevSysUser = ftSysUser;
                //prevSysTotal = ftSysKernel + ftSysUser;

                prevProcKernel = ftProcKernel;
                prevProcUser = ftProcUser;

                if (!NativeMethods.GetProcessIoCounters(hProcess, out ic))
                {
                    Log("NativeMethods.GetProcessIoCounters / GetLastError : " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                    return;
                }

                readOperationCount.Value = ic.ReadOperationCount - prevReadOperationCount;
                writeOperationCount.Value = ic.WriteOperationCount - prevWriteOperationCount;
                otherOperationCount.Value = ic.OtherOperationCount - prevOtherOperationCount;
                //readTransferCount.Value = ic.ReadTransferCount - prevReadTransferCount;
                //writeTransferCount.Value = ic.WriteTransferCount - prevWriteTransferCount;
                //otherTransferCount.Value = ic.OtherTransferCount - prevOtherTransferCount;

                prevReadOperationCount = ic.ReadOperationCount;
                prevWriteOperationCount = ic.WriteOperationCount;
                prevOtherOperationCount = ic.OtherOperationCount;
                prevReadTransferCount = ic.ReadTransferCount;
                prevWriteTransferCount = ic.WriteTransferCount;
                prevOtherTransferCount = ic.OtherTransferCount;

                if (!NativeMethods.GetProcessMemoryInfo(hProcess, out pmc, checked((UInt32)Marshal.SizeOf(typeof(NativeMethods.PROCESS_MEMORY_COUNTERS)))))
                {
                    Log("NativeMethods.GetProcessMemoryInfo / GetLastError : " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                    return;
                }
                workingSetSize.Value = pmc.WorkingSetSize / (1024 * 1024);
                pagefileUsage.Value = pmc.PagefileUsage / (1024 * 1024);

            }
            else
            {

            }


        }

        private bool AttachProcess(string processImageName)
        {
            //Log("AttachProcess : Hello World\n");

            NativeMethods.PROCESSENTRY32 pe32 = new NativeMethods.PROCESSENTRY32(); ;
            IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

            hProcess = IntPtr.Zero;
            pid = 0;
            processFound = false;
            iterationCount = 0;
            
            
            hProcessSnap = NativeMethods.CreateToolhelp32Snapshot(NativeMethods.TH32CS_SNAPPROCESS, 0);
            if (hProcessSnap == INVALID_HANDLE_VALUE)
            {
                //Log("(hProcessSnap == INVALID_HANDLE_VALUE) / GetLastError : " + System.Runtime.InteropServices.Marshal.GetLastWin32Error() + "\n");
                return false;
            }
            else if (hProcessSnap == IntPtr.Zero)
            {
                //Log("(hProcessSnap == IntPtr.Zero) / GetLastError : " + System.Runtime.InteropServices.Marshal.GetLastWin32Error() + "\n");
                return false;
            }
            else
            {
                //Log("hProcessSnap : " + hProcessSnap + " / GetLastError : " + System.Runtime.InteropServices.Marshal.GetLastWin32Error() + "\n");
            }
            
            pe32.dwSize = checked((UInt32)Marshal.SizeOf(typeof(NativeMethods.PROCESSENTRY32)));
            //Log("pe32.dwSize : " + pe32.dwSize + "\n");
            
            if (!NativeMethods.Process32First(hProcessSnap, ref pe32))
            {
                //Log("!NativeMethods.Process32First(hProcessSnap, ref pe32) / GetLastError : " + System.Runtime.InteropServices.Marshal.GetLastWin32Error() + "\n");
                NativeMethods.CloseHandle(hProcessSnap);
                return false;
            }
            else
            {
                //Log("NativeMethods.Process32First(hProcessSnap, ref pe32) / GetLastError : " + System.Runtime.InteropServices.Marshal.GetLastWin32Error() + "\n");
            }
            
            do{
                iterationCount++;
                //Log(" A : " + processImageName + ", B :" + pe32.szExeFile + "\n");
                if (String.Equals(processImageName.ToLower(), pe32.szExeFile.ToLower()))
                {
                    //pick only one process
                    //Log("  (String.Equals(processImageName.ToLower(), pe32.szExeFile.ToLower())\n");
                    hProcess = NativeMethods.OpenProcess(NativeMethods.PROCESS_QUERY_INFORMATION, false, pe32.th32ProcessID);
                    //Log("  OpenProcess / GetLastError : " + System.Runtime.InteropServices.Marshal.GetLastWin32Error() + "\n");
                    pid = pe32.th32ProcessID;
                    processFound = true;
                    //Log("  pe32.szExeFile : " + pe32.szExeFile + " / iterationCount : " + iterationCount + "\n");
                    break;
                }

            } while (NativeMethods.Process32Next(hProcessSnap, ref pe32));
            
            NativeMethods.CloseHandle(hProcessSnap);

            if (hProcess == IntPtr.Zero)
            {
                Log("(hProcess == IntPtr.Zero) / AttachProcess failed\n");
                return false;
            }

            return true;
        }

        private void Log(string s)
        {
#if DEBUG
            System.Console.Write(s);
#endif
        }

        private UInt64 SubtractTimes(ref System.Runtime.InteropServices.ComTypes.FILETIME ftA, ref System.Runtime.InteropServices.ComTypes.FILETIME ftB)
        {
            NativeMethods.LARGE_INTEGER a = new NativeMethods.LARGE_INTEGER();
            NativeMethods.LARGE_INTEGER b = new NativeMethods.LARGE_INTEGER();
	        a.LowPart = (UInt32)ftA.dwLowDateTime;
	        a.HighPart = ftA.dwHighDateTime;

            b.LowPart = (UInt32)ftB.dwLowDateTime;
	        b.HighPart = ftB.dwHighDateTime;

	        return (UInt64)(a.QuadPart - b.QuadPart);
        }


        private class NativeMethods
        {

            [StructLayout(LayoutKind.Explicit, Size = 8)]
            public struct LARGE_INTEGER
            {
                [FieldOffset(0)]
                public Int64 QuadPart;
                [FieldOffset(0)]
                public UInt32 LowPart;
                [FieldOffset(4)]
                public Int32 HighPart;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct OSVersionInfo {
                public Int32 dwOSVersionInfoSize;
                public Int32 dwMajorVersion;
                public Int32 dwMinorVersion;
                public Int32 dwBuildNumber;
                public Int32 dwPlatformId;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public String szCSDVersion;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct MemoryStatusEx {
                public UInt32 Length;
                public UInt32 MemoryLoad;
                public UInt64 TotalPhysicalMemory;
                public UInt64 AvailablePhysicalMemory;
                public UInt64 TotalPageFile;
                public UInt64 AvailPageFile;
                public UInt64 TotalVirtual;
                public UInt64 AvailVirtual;
                public UInt64 AvailExtendedVirtual;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct PROCESS_MEMORY_COUNTERS {
                public UInt32 cb;
                public UInt32 PageFaultCount;
                public UInt64 PeakWorkingSetSize;
                public UInt64 WorkingSetSize;
                public UInt64 QuotaPeakPagedPoolUsage;
                public UInt64 QuotaPagedPoolUsage;
                public UInt64 QuotaPeakNonPagedPoolUsage;
                public UInt64 QuotaNonPagedPoolUsage;
                public UInt64 PagefileUsage;
                public UInt64 PeakPagefileUsage;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct IO_COUNTERS {
                public UInt64 ReadOperationCount;
                public UInt64 WriteOperationCount;
                public UInt64 OtherOperationCount;
                public UInt64 ReadTransferCount;
                public UInt64 WriteTransferCount;
                public UInt64 OtherTransferCount;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct PROCESSENTRY32
            {
                public UInt32 dwSize;
                public UInt32 cntUsage;
                public UInt32 th32ProcessID;
                public IntPtr th32DefaultHeapID;
                public UInt32 th32ModuleID;
                public UInt32 cntThreads;
                public UInt32 th32ParentProcessID;
                public Int32 pcPriClassBase;
                public UInt32 dwFlags;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string szExeFile;
            }; 

            [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern IntPtr GetCurrentProcess();


            [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GlobalMemoryStatusEx(ref NativeMethods.MemoryStatusEx buffer);


            public const Int32 PROCESS_QUERY_INFORMATION = 0x0400;
            [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern IntPtr OpenProcess(Int32 Access, Boolean InheritHandle, UInt32 ProcessId);

            [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetSystemTimes(out System.Runtime.InteropServices.ComTypes.FILETIME lpIdleTime,
                out System.Runtime.InteropServices.ComTypes.FILETIME lpKernelTime,
                out System.Runtime.InteropServices.ComTypes.FILETIME lpUserTime);

            [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetProcessTimes(IntPtr hProcess,
                out System.Runtime.InteropServices.ComTypes.FILETIME lpCreationTime,
                out System.Runtime.InteropServices.ComTypes.FILETIME lpExitTime,
                out System.Runtime.InteropServices.ComTypes.FILETIME lpKernelTime,
                out System.Runtime.InteropServices.ComTypes.FILETIME lpUserTime);

            [System.Runtime.InteropServices.DllImport("psapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetProcessMemoryInfo(IntPtr hProcess, out PROCESS_MEMORY_COUNTERS counters, UInt32 size);

            [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetProcessIoCounters(IntPtr hProcess, out IO_COUNTERS lpIoCounters);

            [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern UInt64 GetTickCount64();

            [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

            [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool QueryPerformanceFrequency(out long lpFrequency);

            public const int TH32CS_SNAPPROCESS = 0x00000002;
            [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processid);

            [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

            [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

            [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CloseHandle(IntPtr hObject);
        }
    }
}
