using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWPortable.PowerPC
{
    public class Instruction
    {
        private static uint[] BitMasks =
        {
            0x00000000,
            0x00000001,
            0x00000003,
            0x00000007,
            0x0000000F,
            0x0000001F,
            0x0000003F,
            0x0000007F,
            0x000000FF,
            0x000001FF,
            0x000003FF,
            0x000007FF,
            0x00000FFF,
            0x00001FFF,
            0x00003FFF,
            0x00007FFF,
            0x0000FFFF,
            0x0001FFFF,
            0x0003FFFF,
            0x0007FFFF,
            0x000FFFFF,
            0x001FFFFF,
            0x003FFFFF,
            0x007FFFFF,
            0x00FFFFFF,
            0x01FFFFFF,
            0x03FFFFFF,
            0x07FFFFFF,
            0x0FFFFFFF,
            0x1FFFFFFF,
            0x3FFFFFFF,
            0x7FFFFFFF,
            0xFFFFFFFF
        };

        public enum ArgType
        {
            INVALID = 0,
            RESERVED,
            BOOL,
            FLOATREGISTER,
            SPECIALREGISTER,
            CONDITIONREGISTER,
            REGISTER,
            IMMEDIATE,
            BRANCHIMMEDIATE,
            BRANCHIMMEDIATERELATIVE
        };

        public struct Arg
        {
            public ArgType Type;
            public int Index;
            public int Length;
        }

        private List<PPCError> _errors = new List<PowerPC.PPCError>();
        private string _line = null;

        public string Name;
        public Arg[] Args;
        public uint Opcode;
        public int[] Shifts;
        public int Mask;

        public Instruction(string ins, uint op, params Arg[] args)
        {
            int shift = 26;

            Args = args;
            Opcode = op;
            Name = ins;
            Mask = -1;

            // Parse shifts
            Shifts = new int[args.Length];
            for (int x = 0; x < args.Length; x++)
            {
                // Update mask
                if (args[x].Type != ArgType.RESERVED)
                {
                    for (int s = shift; s > (shift - args[x].Length); s--)
                        Mask &= ~(args[x].Length << s);
                }

                shift -= args[x].Length;
                Shifts[x] = shift;
            }
        }

        public uint ParseValue(string value, int bitLength, uint offset = 0)
        {
            uint ret = 0;

            if (bitLength < 0 || bitLength > 31)
                return 0;

            value = value.ToLower();

            // Hexadecimal immediates
            if (value.IndexOf("0x") == 0)
                try { ret = Convert.ToUInt32(value.Substring(2), 16); goto _parseValue_End; } catch { goto _parseValue_Error; }

            // Special registers
            if (value == "lr") { ret = 8; goto _parseValue_End; }
            if (value == "xer") { ret = 1; goto _parseValue_End; }
            if (value == "ctr") { ret = 9; goto _parseValue_End; }

            // General Purpose Register
            if (value.IndexOf("r") == 0)
                try { ret = Convert.ToUInt32(value.Substring(1)) & 0x1F; goto _parseValue_End; } catch { goto _parseValue_Error; }

            // Floating Point Register
            if (value.IndexOf("f") == 0)
                try { ret = Convert.ToUInt32(value.Substring(1)) & 0x1F; goto _parseValue_End; } catch { goto _parseValue_Error; }

            // Conditional Registers
            if (value.IndexOf("crb") == 0)
                try { ret = (Convert.ToUInt32(value.Substring(3)) & 0x7) << (bitLength-3); goto _parseValue_End; } catch { goto _parseValue_Error; }

            if (value.IndexOf("cr") == 0)
                try { ret = (Convert.ToUInt32(value.Substring(2)) & 0x7) << (bitLength-3); goto _parseValue_End; } catch { goto _parseValue_Error; }

            // Decimal immediate
            try { ret = Convert.ToUInt32(value); goto _parseValue_End; } catch { }

            // Error, invalid argument
            _parseValue_Error:;
            _errors.Add(new PowerPC.PPCError("Invalid argument \"" + value + "\"", _line));
            return 0;

            _parseValue_End:;
            return (ret-offset) & BitMasks[bitLength];
        }

        public List<PPCError> Assemble(uint address, string line, out PPCResult ppc)
        {
            int x, y;
            uint r = Opcode;
            string[] args = line.Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            _errors.Clear();
            _line = line;

            // Here we loop through each argument and OR them into the final result
            // Registers, condition registers, and 5 bit immediates can be in different orders so we shift them according to their index
            for (x = 0, y = 0; x < (args.Length-1); x++, y++)
            {
                // Skip reserved bits
                while (Args[y].Type == ArgType.RESERVED)
                    y++;

                switch (Args[y].Type)
                {
                    case ArgType.REGISTER:                              // Register (5 bits, "r0,r1,...,r31")
                        r |= (ParseValue(args[x + 1], Args[y].Length) & 0x1F) << Shifts[Args[y].Index];
                        break;
                    case ArgType.CONDITIONREGISTER:                     // Condition Register (3 bits, "cr0,cr1,...,cr7" or 5 bits, "0,4,8,...,28")
                        r |= ParseValue(args[x + 1], Args[y].Length) << Shifts[Args[y].Index];
                        break;
                    case ArgType.SPECIALREGISTER:                       // Special Register (5 bits, "xer,ctr,lr"
                        r |= ParseValue(args[x + 1], Args[y].Length) << Shifts[Args[y].Index];
                        break;
                    case ArgType.BOOL:                                  // Boolean (1 bit, "0,1")
                        r |= (uint)(args[x + 1] == "1"?1:0) << Shifts[Args[y].Index];
                        break;
                    case ArgType.IMMEDIATE:
                        r |= ParseValue(args[x + 1], Args[y].Length) << Shifts[Args[y].Index];
                        break;
                    case ArgType.BRANCHIMMEDIATE:
                        r |= (ParseValue(args[x + 1], Args[y].Length) >> 2) << Shifts[Args[y].Index];
                        break;
                    case ArgType.BRANCHIMMEDIATERELATIVE:
                        r |= (ParseValue(args[x + 1], Args[y].Length, address) >> 2) << Shifts[Args[y].Index];
                        break;
                }
            }

            ppc = new PPCResult(address, r);
            return _errors;
        }
    }
}
