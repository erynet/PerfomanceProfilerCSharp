using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using EasyHook;
using Direct3DHookLib.Interface;
using Direct3DHookLib.Hook;
using Direct3DHookLib;


namespace MonitorLib.Hardware.DirectX
{
    internal class GenericDirectX : Hardware
    {
        private Int32 Pid { get; set; }
        private float DxVersion { get; set; }
        private float Fps { get; set; }
        //private Sensor loadSensor;
        //private Sensor usedMemory;
        //private Sensor availableMemory;
        private Sensor pidSensor;
        private Sensor dxVersionSensor;
        private Sensor fpsSensor;

        private Sensor naSensor;

        private bool isAttached;
        private CaptureProcess _captureProcess;

        private TimeSpan fpsTickTimeSpan;
        private Int32 fpsTickCount;

        private StreamWriter fpsTimecodeWriter;
        private string fpsTimecodeWriterBuffer;

        public GenericDirectX(string processImageName, ISettings settings) : base("DirectX", new Identifier("DirectX"), settings)
        {
            //loadSensor = new Sensor("Memory", 0, SensorType.Load, this, settings);
            //ActivateSensor(loadSensor);

            //usedMemory = new Sensor("Used Memory", 0, SensorType.Data, this, settings);
            //ActivateSensor(usedMemory);

            //availableMemory = new Sensor("Available Memory", 1, SensorType.Data, this, settings);
            //ActivateSensor(availableMemory);

            fpsTickTimeSpan = new TimeSpan();

            if (AttachProcess(processImageName))
            {
                isAttached = true;
                CreateTimecodeLog();

                AppDomain.CurrentDomain.ProcessExit += new EventHandler(UnHookOnAppExit);

                pidSensor = new Sensor("Process Id", 0, SensorType.Comment, this, settings);
                ActivateSensor(pidSensor);
                dxVersionSensor = new Sensor("DirectX Version", 1, SensorType.Comment, this, settings);
                ActivateSensor(dxVersionSensor);

                fpsSensor = new Sensor("FPS(Present)", 0, SensorType.FPS, this, settings);
                ActivateSensor(fpsSensor);
            }
            else
            {
                isAttached = false;
                naSensor = new Sensor("N/A", 0, SensorType.Comment, this, settings);
                ActivateSensor(naSensor);
            }
        }

        public override HardwareType HardwareType
        {
            get
            {
                return HardwareType.DirectX;
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
            if (isAttached)
            {
                pidSensor.Value = Pid;
                dxVersionSensor.Value = DxVersion;
                fpsSensor.Value = Fps;
            }
            
        }

        private bool AttachProcess(string processImageName)
        {
            LogN("Attempt Directx Hook Attach");
            LogN("processImageName : " + processImageName);
            string exeName = Path.GetFileNameWithoutExtension(processImageName);
            LogN("exeName : " + exeName);

            Process[] processes = Process.GetProcessesByName(exeName);
            
            LogN("processes.Length : " + processes.Length);
            foreach(Process process in processes)
            {
                try {
                    if (process.MainWindowHandle == IntPtr.Zero)
                    {
                        continue;
                    }
                    if (HookManager.IsHooked(process.Id))
                    {
                        continue;
                    }

                    CaptureConfig cc = new CaptureConfig()
                    {
                        Direct3DVersion = Direct3DVersion.AutoDetect,
                        ShowOverlay = true
                    };
                    var captureInterface = new CaptureInterface();
                    captureInterface.RemoteMessage += new MessageReceivedEvent(CaptureInterface_RemoteMessage);
                    captureInterface.RemoteReportProperty += new ReportPropertyReceivedEvent(CaptureInterface_RemoteReportProperty);
                    captureInterface.RemoteReportFps += new ReportFpsReceivedEvent(CaptureInterface_RemoteReportFps);
                    captureInterface.RemoteReportFpsTimecode += new ReportFpsTimecodeReceivedEvent(CaptureInterface_RemoteReportFpsTimecode);
                    _captureProcess = new CaptureProcess(process, cc, captureInterface);
                    
                    break;
                }
                catch 
                {
                    LogN("Maybe Access deinied : " + process.ProcessName);
                }
            }
            Thread.Sleep(10);

            if (_captureProcess == null)
            {
                LogN("Attempt Directx Hook Attach : Failed");
                return false;
            }
            else
            {
                LogN("Attempt Directx Hook Attach : Success");
                return true;

            }
        }


        private void Log(string s)
        {
#if DEBUG
            System.Console.Write(s);
#endif
        }
        private void LogN(string s)
        {
            Log(s + "\n");
        }


        void CaptureInterface_RemoteMessage(MessageReceivedEventArgs message)
        {
#if DEBUG
            System.Console.WriteLine(String.Format("{0}", message));
#endif
        }

        void CaptureInterface_RemoteReportProperty(ReportPropertyReceivedEventArgs prop)
        {
            
            Pid = prop.Pid;
            DxVersion = prop.DxVersion;
#if DEBUG
            System.Console.WriteLine("Pid : " + Pid);
            System.Console.WriteLine("DxVersion : " + DxVersion);
#endif
        }

        void CaptureInterface_RemoteReportFps(ReportFpsReceivedEventArgs prop)
        {
            Fps = prop.Fps;

            //System.Console.WriteLine("Fps : " + Fps);
        }

        void CaptureInterface_RemoteReportFpsTimecode(ReportFpsTimecodeReceivedEventArgs tcode)
        {
            WriteTimecodeLog(tcode.Ticks);
#if DEBUG
            //string temp = String.Format("{0}:{1}:{2}.{3}", fpsTickTimeSpan.Hours, fpsTickTimeSpan.Minutes, fpsTickTimeSpan.Seconds, fpsTickTimeSpan.Milliseconds);
            
            //System.Console.WriteLine("CaptureInterface_RemoteReportFpsTimecode / T : " + temp);
#endif
        }

        void CreateTimecodeLog()
        {
            fpsTickCount = 0;
            fpsTimecodeWriterBuffer = "";
            var now = DateTime.Now;
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Log"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Log");
            }
            fpsTimecodeWriter = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Log" + Path.DirectorySeparatorChar + string.Format("TimeCode-{0:yyyy-MM-dd-H-mm-ss}.csv", now), false);
        }

        void WriteTimecodeLog(Int64 Tick)
        {
            fpsTickCount += 1;

            fpsTickTimeSpan = TimeSpan.FromTicks(Tick);
            fpsTimecodeWriterBuffer += String.Format("{0,2:D2}:{1,2:D2}:{2,2:D2}.{3,3:D3},", fpsTickTimeSpan.Hours, fpsTickTimeSpan.Minutes, fpsTickTimeSpan.Seconds, fpsTickTimeSpan.Milliseconds);
            fpsTimecodeWriterBuffer += System.Environment.NewLine;
            
            if ((fpsTickCount % 30) == 0)
            {
                fpsTimecodeWriter.Write(fpsTimecodeWriterBuffer);
                fpsTimecodeWriterBuffer = "";
                fpsTimecodeWriter.Flush();
            }
        }

        void UnHookOnAppExit(object sender, EventArgs e)
        {
            _captureProcess.CaptureInterface.Disconnect();
        }
    }
}
