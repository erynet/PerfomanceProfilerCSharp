﻿using System;
using System.Collections.Generic;
using MonitorLib.Hardware;
using ProfilerCS.Utilities;

namespace ProfilerCS.GUI {
  public class SensorNode : Node {
    
    private ISensor sensor;
    private PersistentSettings settings;
    private UnitManager unitManager;
    private string format;
    private bool plot = false;       

    public string ValueToString(float? value) {
      if (value.HasValue) {
        if (sensor.SensorType == SensorType.Temperature && 
          unitManager.TemperatureUnit == TemperatureUnit.Fahrenheit) {
          return string.Format("{0:F1} °F", value * 1.8 + 32);
        } else {
          return string.Format(format, value);
        }                
      } else
        return "-";
    }

    public SensorNode(ISensor sensor, PersistentSettings settings, 
      UnitManager unitManager) : base() {      
      this.sensor = sensor;
      this.settings = settings;
      this.unitManager = unitManager;
      switch (sensor.SensorType) {
        case SensorType.Voltage: format = "{0:F3} V"; break;
        case SensorType.Clock: format = "{0:F0} MHz"; break;
        case SensorType.Load: format = "{0:F1} %"; break;
        case SensorType.Temperature: format = "{0:F1} °C"; break;
        case SensorType.Fan: format = "{0:F0} RPM"; break;
        case SensorType.Flow: format = "{0:F0} L/h"; break;
        case SensorType.Control: format = "{0:F1} %"; break;
        case SensorType.Level: format = "{0:F1} %"; break;
        case SensorType.Power: format = "{0:F1} W"; break;
        case SensorType.Data: format = "{0:F1} GB"; break;
        case SensorType.Factor: format = "{0:F3}"; break;
        case SensorType.FPS: format = "{0:F1} F/s"; break;
        case SensorType.Comment: format = "{0}"; break;
        case SensorType.Data2: format = "{0} MB"; break;
        case SensorType.IOPS: format = "{0} IOPS"; break;

      }

      bool hidden = settings.GetValue(new Identifier(sensor.Identifier, 
        "hidden").ToString(), sensor.IsDefaultHidden);
      base.IsVisible = !hidden;

      this.Plot = settings.GetValue(new Identifier(sensor.Identifier, 
        "plot").ToString(), false);
    }

    public override string Text {
      get { return sensor.Name; }
      set { sensor.Name = value; }
    }

    public override bool IsVisible {
      get { return base.IsVisible; }
      set { 
        base.IsVisible = value;
        settings.SetValue(new Identifier(sensor.Identifier,
          "hidden").ToString(), !value);
      }
    }

    public bool Plot {
      get { return plot; }
      set { 
        plot = value;
        settings.SetValue(new Identifier(sensor.Identifier, "plot").ToString(), 
          value);
        if (PlotSelectionChanged != null)
          PlotSelectionChanged(this, null);
      }
    }

    public event EventHandler PlotSelectionChanged;

    public ISensor Sensor {
      get { return sensor; }
    }

    public string Value {
      get { return ValueToString(sensor.Value); }
    }

    public string Min {
      get { return ValueToString(sensor.Min); }
    }

    public string Max {
      get { return ValueToString(sensor.Max); }
    }

    public override bool Equals(System.Object obj) {
      if (obj == null) 
        return false;

      SensorNode s = obj as SensorNode;
      if (s == null) 
        return false;

      return (sensor == s.sensor);
    }

    public override int GetHashCode() {
      return sensor.GetHashCode();
    }

  }
}
