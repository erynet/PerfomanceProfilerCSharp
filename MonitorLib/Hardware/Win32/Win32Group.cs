using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonitorLib.Hardware.Win32 {
    internal class Win32Group : IGroup
    {

        private Hardware[] hardware;

        public Win32Group(ISettings settings) {

            // No implementation for RAM on Unix systems
            int p = (int)Environment.OSVersion.Platform;
            if ((p == 4) || (p == 128)) {
                hardware = new Hardware[0];
                return;
            }

            //if (Environment.GetCommandLineArgs().Length >= 2)
            //{
            hardware = new Hardware[] { new GenericWin32(Environment.GetCommandLineArgs()[1], settings) };
            //}
        }

        public string GetReport() {
            return null;
        }

        public IHardware[] Hardware {
            get {
                return hardware;
            }
        }

        public void Close() {
            foreach (Hardware win32object in hardware)
                win32object.Close();
        }
    }
}
