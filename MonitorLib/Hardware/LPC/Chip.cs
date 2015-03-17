﻿/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2013 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

namespace MonitorLib.Hardware.LPC {

  internal enum Chip : ushort {
    Unknown = 0,

    ATK0110 = 0x0110,

    F71858 = 0x0507,
    F71862 = 0x0601, 
    F71869 = 0x0814,
    F71869A = 0x1007,
    F71882 = 0x0541,
    F71889AD = 0x1005,
    F71889ED = 0x0909,
    F71889F = 0x0723,
    F71808E = 0x0901,

    IT8705F = 0x8705,
    IT8712F = 0x8712,
    IT8716F = 0x8716,
    IT8718F = 0x8718,
    IT8720F = 0x8720,
    IT8721F = 0x8721,
    IT8726F = 0x8726,
    IT8728F = 0x8728,
    IT8771E = 0x8771,
    IT8772E = 0x8772,

    NCT6771F = 0xB470,
    NCT6776F = 0xC330,
    NCT6779D = 0xC560,
    NCT6791D = 0xC803,

    W83627DHG = 0xA020,
    W83627DHGP = 0xB070,
    W83627EHF = 0x8800,    
    W83627HF = 0x5200,
    W83627THF = 0x8280,
    W83667HG = 0xA510,
    W83667HGB = 0xB350,
    W83687THF = 0x8541
  }

  internal class ChipName {

    private ChipName() { }

    public static string GetName(Chip chip) {
      switch (chip) {
        case Chip.ATK0110: return "Asus ATK0110";

        case Chip.F71858: return "Fintek F71858";
        case Chip.F71862: return "Fintek F71862";
        case Chip.F71869: return "Fintek F71869";
        case Chip.F71869A: return "Fintek F71869A";
        case Chip.F71882: return "Fintek F71882";
        case Chip.F71889AD: return "Fintek F71889AD";
        case Chip.F71889ED: return "Fintek F71889ED";
        case Chip.F71889F: return "Fintek F71889F";
        case Chip.F71808E: return "Fintek F71808E";

        case Chip.IT8705F: return "ITE IT8705F";
        case Chip.IT8712F: return "ITE IT8712F";
        case Chip.IT8716F: return "ITE IT8716F";
        case Chip.IT8718F: return "ITE IT8718F";        
        case Chip.IT8720F: return "ITE IT8720F";
        case Chip.IT8721F: return "ITE IT8721F";
        case Chip.IT8726F: return "ITE IT8726F";
        case Chip.IT8728F: return "ITE IT8728F";
        case Chip.IT8771E: return "ITE IT8771E";
        case Chip.IT8772E: return "ITE IT8772E";

        case Chip.NCT6771F: return "Nuvoton NCT6771F";
        case Chip.NCT6776F: return "Nuvoton NCT6776F";
        case Chip.NCT6779D: return "Nuvoton NCT6779D";
        case Chip.NCT6791D: return "Nuvoton NCT6791D";

        case Chip.W83627DHG: return "Winbond W83627DHG";
        case Chip.W83627DHGP: return "Winbond W83627DHG-P";
        case Chip.W83627EHF: return "Winbond W83627EHF";
        case Chip.W83627HF: return "Winbond W83627HF";
        case Chip.W83627THF: return "Winbond W83627THF";
        case Chip.W83667HG: return "Winbond W83667HG";
        case Chip.W83667HGB: return "Winbond W83667HG-B";
        case Chip.W83687THF: return "Winbond W83687THF";

        case Chip.Unknown: return "Unkown";
        default: return "Unknown";
      }
    }
  }

}