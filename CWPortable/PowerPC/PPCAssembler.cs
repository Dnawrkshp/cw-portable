﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CWPortable.PowerPC
{
    public class PPCAssembler
    {
        public PPCAssembler()
        {
            // If the PowerPC extensions directory doesn't already exist, install the standard extension commands
            // Otherwise, load them
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CWPortable", "PowerPC", "Extensions");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);

                //File.WriteAllText(Path.Combine(path, "setreg.txt"), Properties.Resources.SETREG);
                //File.WriteAllText(Path.Combine(path, "mr.txt"), Properties.Resources.MR);
            }
            File.WriteAllText(Path.Combine(path, "setreg.txt"), Properties.Resources.SETREG);
            File.WriteAllText(Path.Combine(path, "mr.txt"), Properties.Resources.MR);
            File.WriteAllText(Path.Combine(path, "lis.txt"), Properties.Resources.LIS);
            File.WriteAllText(Path.Combine(path, "li.txt"), Properties.Resources.LI);
            File.WriteAllText(Path.Combine(path, "sub.txt"), Properties.Resources.SUB);
            File.WriteAllText(Path.Combine(path, "beq.txt"), Properties.Resources.BEQ);
            File.WriteAllText(Path.Combine(path, "blt.txt"), Properties.Resources.BLT);
            File.WriteAllText(Path.Combine(path, "ble.txt"), Properties.Resources.BLE);
            File.WriteAllText(Path.Combine(path, "bgt.txt"), Properties.Resources.BGT);
            File.WriteAllText(Path.Combine(path, "bge.txt"), Properties.Resources.BGE);

            if (Definitions.Extensions == null)
                Definitions.LoadExtensions(path);

            if (Definitions.Instructions == null)
                Definitions.Initialize();

        }

        public List<PPCError> Assemble(string text, out List<PPCResult> ppc)
        {
            ppc = new List<PowerPC.PPCResult>();
            List<PPCError> errors = new List<PowerPC.PPCError>();
            PPCResult r;
            string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] subLines;
            uint address = 0;
            int x, y;
            bool found = false;
            
            // Preprocess all the extensions
            for (x = 0; x < lines.Length; x++)
            {
                found = false;
                subLines = lines[x].Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (y = 0; y < subLines.Length; y++)
                {
                    foreach (Extended ex in Definitions.Extensions)
                    {
                        if (subLines[y].StartsWith(ex.Name + " "))
                        {
                            PPCError error = ex.Assemble(ref subLines[y]);
                            if (error != null)
                            {
                                error.Address = 0xFFFFFFFF;
                                error.Line = x;
                                errors.Add(error);
                            }
                            found = true;
                        }
                    }
                }

                lines[x] = String.Join("\r\n", subLines);

                if (found)
                    x--;
            }

            for (x = 0; x < lines.Length; x++)
            {
                subLines = lines[x].Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                for (y = 0; y < subLines.Length; y++)
                {
                    found = false;
                    if (subLines[y].StartsWith("hexcode "))
                    {
                        ppc.Add(new PPCResult(address, subLines[y].Split(' ')[1]));
                        address += 4;
                        found = true;
                    }
                    else
                    {
                        foreach (Instruction ins in Definitions.Instructions)
                        {
                            if (subLines[y].StartsWith(ins.Name + " "))
                            {
                                // Assemble and update Line/Address parameters of each error
                                List<PPCError> asmErrors = ins.Assemble(address, subLines[y], out r);
                                foreach (PPCError e in asmErrors)
                                {
                                    e.Address = address;
                                    e.Line = x;
                                }

                                errors.AddRange(asmErrors);

                                r.Address = address;
                                ppc.Add(r);
                                address += 4;
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                        errors.Add(new PPCError("Invalid instruction \"" + subLines[y].Split(' ')[0], null, x));
                }
            }

            return errors;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="hexCodes"></param>
        /// <returns></returns>
        public string Disassemble(string[] hexCodes)
        {
            return "";
        }

    }
}
