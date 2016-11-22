using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CWPortable.PowerPC
{
    public class PPCError
    {
        public Exception Ex;
        public string Error;
        public string Context;
        public int Line;
        public int Column;
        public uint Address;

        public PPCError(string error, string context = null, int line = -1, int column = -1)
        {
            Error = error;
            Context = context;
            Line = line;
            Column = column;
            Ex = null;
        }

        public PPCError(Exception error, string context = null, int line = -1, int column = -1)
        {
            Error = null;
            Context = context;
            Line = line;
            Column = column;
            Ex = error;
        }

        public override string ToString()
        {
            string output = "";

            if (Context != null)
                output += "In " + Context + " ";
            if (Line >= 0)
                output += "Line: " + Line.ToString() + " ";
            if (Column >= 0)
                output += "Column: " + Column.ToString() + " ";
            if (Address != 0xFFFFFFFF)
                output += "Address: " + Address.ToString("X8") + " ";

            output += "\r\n";

            if (Error != null)
                output += "Error: " + Error + "\r\n";
            if (Ex != null)
                output += (Ex.Source == null ? "" : (Ex.Source + ": ")) + (Ex.Message == null ? "" : Ex.Message) + "\r\n";

            return output.Trim(' ', '\r', '\n');
        }
    }
}
