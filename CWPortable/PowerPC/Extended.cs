using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;


namespace CWPortable.PowerPC
{
    public class Extended
    {
        private static CSharpCodeProvider _provider = null;
        private static CompilerParameters _parameters = null;

        private const string _codeTop = "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing System.Text;\r\n\r\nnamespace CWPortable.PowerPC {\r\npublic static class Ex {\r\npublic static void Main() { }\r\n";
        private const string _codeBottom = "\r\n} }";

        private MethodInfo _assemble = null;
        private bool _changed = false;
        private string _code;


        public string Code
        {
            get { return _code; }
            set { _code = value; _changed = true; }
        }

        public string Name;

        public Extended(string name, string code)
        {
            Name = name;
            Code = code;

            if (_provider == null)
                _provider = new CSharpCodeProvider();
            if (_parameters == null)
            {
                _parameters = new CompilerParameters();

                // Reference libraries
                _parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
                _parameters.ReferencedAssemblies.Add("System.Core.dll");
                _parameters.ReferencedAssemblies.Add("System.Data.dll");

                // Build exe in memory
                _parameters.GenerateInMemory = true;
                _parameters.GenerateExecutable = true;
            }
        }

        public string Build()
        {
            CompilerResults results = _provider.CompileAssemblyFromSource(_parameters, _codeTop + _code + _codeBottom);

            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();

                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine(String.Format("Error ({0}) at ({1},{2}): {3}", error.ErrorNumber, error.Line, error.Column, error.ErrorText));
                }
                
                return sb.ToString();
            }

            Assembly assembly = results.CompiledAssembly;

            Type program = assembly.GetType("CWPortable.PowerPC.Ex");
            _assemble = program.GetMethod("Assemble");

            return null;
        }

        public PPCError Assemble(ref string line)
        {
            if (_assemble == null || _changed)
            {
                string build = Build();
                if (build != null)
                    return new PowerPC.PPCError(build, "Extension " + this.Name);
            }

            try
            {
                line = (string)_assemble.Invoke(null, new object[] { line.Split(new char[] { ' ' }) });
            }
            catch (Exception e) { return new PPCError(e, "Extension " + this.Name); }

            return null;
        }

    }
}
