using System;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace csharp_tests
{
    public class CodeCompiler
    {
        protected CSharpCodeProvider compiler = new CSharpCodeProvider();
        protected CompilerParameters compiler_parameters;

        public CodeCompiler()
        {
            this.compiler_parameters = new CompilerParameters
            {
                WarningLevel = 0,
                GenerateExecutable = false,
                GenerateInMemory = true/*,
                OutputAssembly = "Assembly.dll"*/
            };
        }

        public void AddReferencedAssembly(string name)
        {
            this.compiler_parameters.ReferencedAssemblies.Add(name);
        }
         
        public Assembly compileFile(string file)
        {
            Assembly returnAssembly = null;
            try
            {
                CompilerResults results = this.compiler.CompileAssemblyFromFile(compiler_parameters, file);
                returnAssembly = results.CompiledAssembly;
         
                foreach (string output in results.Output)
                {
                    Console.WriteLine(output);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            return returnAssembly;
        }
    }
}

