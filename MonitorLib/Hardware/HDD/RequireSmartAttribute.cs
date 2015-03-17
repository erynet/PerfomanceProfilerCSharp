﻿using System;
using System.Collections.Generic;

namespace MonitorLib.Hardware.HDD
{

  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  internal class RequireSmartAttribute : Attribute {

    public RequireSmartAttribute(byte attributeId) {
      AttributeId = attributeId;
    }

    public byte AttributeId { get; private set; }

  }
}
