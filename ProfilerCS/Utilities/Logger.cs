using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MonitorLib.Hardware;

namespace ProfilerCS.Utilities {
  public class Logger {

    private const string fileNameFormat = "Log-{0:yyyy-MM-dd-H-mm-ss}.csv";

    private readonly IComputer computer;

    private DateTime day = DateTime.MinValue;
    private string fileName;
    private string[] identifiers;
    private ISensor[] sensors;

    private DateTime lastLoggedTime = DateTime.MinValue;
    private DateTime lastLogWrittenTime = DateTime.MinValue;

    public Logger(IComputer computer) {
      this.computer = computer;
      this.computer.HardwareAdded += HardwareAdded;
      this.computer.HardwareRemoved += HardwareRemoved;
      LogBuffer = "";
    }

    private void HardwareRemoved(IHardware hardware) {
      hardware.SensorAdded -= SensorAdded;
      hardware.SensorRemoved -= SensorRemoved;
      foreach (ISensor sensor in hardware.Sensors)
        SensorRemoved(sensor);
      foreach (IHardware subHardware in hardware.SubHardware)
        HardwareRemoved(subHardware);
    }

    private void HardwareAdded(IHardware hardware) {
      foreach (ISensor sensor in hardware.Sensors)
        SensorAdded(sensor);
      hardware.SensorAdded += SensorAdded;
      hardware.SensorRemoved += SensorRemoved;
      foreach (IHardware subHardware in hardware.SubHardware)
        HardwareAdded(subHardware);
    }

    private void SensorAdded(ISensor sensor) {
      if (sensors == null)
        return;

      for (int i = 0; i < sensors.Length; i++) {
        if (sensor.Identifier.ToString() == identifiers[i])
          sensors[i] = sensor;
      }
    }

    private void SensorRemoved(ISensor sensor) {
      if (sensors == null)
        return;

      for (int i = 0; i < sensors.Length; i++) {
        if (sensor == sensors[i])
          sensors[i] = null;
      }
    }

    private static string GetFileName(DateTime date) {
        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Log"))
        {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Log");
        }

        return AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Log" + Path.DirectorySeparatorChar + string.Format(fileNameFormat, date);
    }

    private bool OpenExistingLogFile() {
      if (!File.Exists(fileName))
        return false;

      try {
        String line;
        using (StreamReader reader = new StreamReader(fileName)) 
          line = reader.ReadLine(); 
       
        if (string.IsNullOrEmpty(line))
          return false;
        
        identifiers = line.Split(',').Skip(1).ToArray();
      } catch {
        identifiers = null;
        return false;
      }

      if (identifiers.Length == 0) {
        identifiers = null;
        return false;
      }

      sensors = new ISensor[identifiers.Length];
      SensorVisitor visitor = new SensorVisitor(sensor => {
        for (int i = 0; i < identifiers.Length; i++)
          if (sensor.Identifier.ToString() == identifiers[i])
            sensors[i] = sensor;
      });
      visitor.VisitComputer(computer);
      return true;
    }

    private void CreateNewLogFile() {
      IList<ISensor> list = new List<ISensor>();
      SensorVisitor visitor = new SensorVisitor(sensor => {
        list.Add(sensor);
      });
      visitor.VisitComputer(computer);
      sensors = list.ToArray();
      identifiers = sensors.Select(s => s.Identifier.ToString()).ToArray();

      using (StreamWriter writer = new StreamWriter(fileName, false)) {
        //writer.Write(",");
        //for (int i = 0; i < sensors.Length; i++)
        //{
        //  writer.Write(sensors[i].Identifier);
        //  if (i < sensors.Length - 1)
        //    writer.Write(",");
        //  else
        //    writer.WriteLine();
        //}

        writer.Write("Time,");
        for (int i = 0; i < sensors.Length; i++) {
          //writer.Write('"');
          writer.Write(sensors[i].Name);
          //writer.Write('"');
          if (i < sensors.Length - 1)
            writer.Write(",");
          else
            writer.WriteLine();
        }
      }
    }

    public TimeSpan LoggingInterval { get; set; }

    private string LogBuffer;

    public void Log() {
      var now = DateTime.Now;
      
      if (lastLoggedTime + LoggingInterval - TimeSpan.FromMilliseconds(30) > now)
        return;      

      if (day != now.Date || !File.Exists(fileName)) {
          day = now.Date;
        //fileName = GetFileName(day);
          fileName = GetFileName(now);

        if (!OpenExistingLogFile())
          CreateNewLogFile();
      }

      //try
      //{
      //    using (StreamWriter writer = new StreamWriter(new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
      //    {
      //        //writer.Write(now.ToString("G", CultureInfo.InvariantCulture));
      //        writer.Write(String.Format("{0,2:D2}:{1,2:D2}:{2,2:D2}.{3,3:D3},", now.TimeOfDay.Hours, now.TimeOfDay.Minutes, now.TimeOfDay.Seconds, now.TimeOfDay.Milliseconds));
      //        writer.Write(",");
      //        for (int i = 0; i < sensors.Length; i++)
      //        {
      //            if (sensors[i] != null)
      //            {
      //                float? value = sensors[i].Value;
      //                if (value.HasValue)
      //                    writer.Write(value.Value.ToString("R", CultureInfo.InvariantCulture));
      //            }
      //            if (i < sensors.Length - 1)
      //                writer.Write(",");
      //            else
      //                writer.WriteLine();
      //        }
      //    }
      //}
      //catch (IOException) { }

      if (now - lastLogWrittenTime > TimeSpan.FromMilliseconds(3000))
      {
          try
          {
              using (StreamWriter writer = new StreamWriter(new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
              {
                  writer.Write(LogBuffer);
                  LogBuffer = "";
                  lastLogWrittenTime = now;
              }
          }
          catch (IOException) { }
      }
      else
      {
          LogBuffer += String.Format("{0,2:D2}:{1,2:D2}:{2,2:D2}.{3,3:D3},", now.TimeOfDay.Hours, now.TimeOfDay.Minutes, now.TimeOfDay.Seconds, now.TimeOfDay.Milliseconds);
          LogBuffer += ",";
          for (int i = 0; i < sensors.Length; i++)
          {
              if (sensors[i] != null)
              {
                  float? value = sensors[i].Value;
                  
                  if (value.HasValue)
                      LogBuffer += value.Value.ToString("R", CultureInfo.InvariantCulture);
              }
              if (i < sensors.Length - 1)
                  LogBuffer += ",";
              else
                  LogBuffer += System.Environment.NewLine;
          }

      }

      lastLoggedTime = now;
    }
  }
}
