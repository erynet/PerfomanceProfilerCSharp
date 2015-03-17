using System;
using System.Collections.Generic;

namespace MonitorLib.Hardware.HDD
{

  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  internal class NamePrefixAttribute : Attribute {

    public NamePrefixAttribute(string namePrefix) {
      Prefix = namePrefix;
    }

    public string Prefix { get; private set; }

  }
}
