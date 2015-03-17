using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonitorLib.Hardware.DirectX {
    internal class DirectXGroup : IGroup
    {

        private Hardware[] hardware;

        public DirectXGroup(ISettings settings)
        {

            // No implementation for RAM on Unix systems
            int p = (int)Environment.OSVersion.Platform;
            if ((p == 4) || (p == 128)) {
                hardware = new Hardware[0];
                return;
            }

            hardware = new Hardware[] { new GenericDirectX(Environment.GetCommandLineArgs()[1], settings) };
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
            foreach (Hardware directxObject in hardware)
                directxObject.Close();
        }
    }
}
