using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CWPortable.PowerPC
{
    public class PPCResult
    {
        public uint Address;
        public uint Hex;

        public PPCResult(uint address, uint code)
        {
            Address = address;
            Hex = code;
        }

        public PPCResult(uint address, string hex)
        {
            Address = address;
            Hex = 0;

            if (hex == null || hex == "")
                return;

            hex = hex.Replace(",", "").ToLower();

            try
            {
                if (hex.StartsWith("r "))
                    Hex = uint.Parse(hex.Substring(1)) & 0x1F;
                else if (hex.StartsWith("f "))
                    Hex = uint.Parse(hex.Substring(1)) & 0x1F;
                else if (hex == "xer")
                    Hex = 1;
                else if (hex == "lr")
                    Hex = 8;
                else if (hex == "ctr")
                    Hex = 9;
                else if (hex.StartsWith("crb "))
                    Hex = uint.Parse(hex.Substring(3, 1)) & 0x7;
                else if (hex.StartsWith("cr "))
                    Hex = uint.Parse(hex.Substring(2, 1)) & 0x7;
                else if (hex.StartsWith("0x"))
                    Hex = (uint)(Convert.ToUInt64(hex.Substring(2), 16) & 0xFFFFFFFF);
                else if (hex.StartsWith(":"))
                    Hex = 0; // Todo: label
                else
                    Hex = (uint)(Convert.ToUInt64(hex) & 0xFFFFFFFF);
            }
            catch { }
        }

        public override string ToString()
        {
            return Address.ToString("X8") + " " + Hex.ToString("X8");
        }

    }
}
