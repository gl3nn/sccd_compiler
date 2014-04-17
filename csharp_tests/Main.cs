using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Xml.Linq;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;

namespace csharp_tests
{
    [TestFixture]
    public class Main
    {
   
        [Test, TestCaseSource("GetTestCases")]
        public void testXMLModel(string file_path)
        {
            XElement test_xml = XDocument.Load(file_path).Root.Element("test");
            Assert.AreNotEqual(null, test_xml, "No test data found. (A test that should just compile correctly, still needs an empthy test tag.)");

            //If the test file expects an exception to be thrown by the compiler,
            //we just try to compile the model and see if a file got generated.
            //If the result file got generated, this means that no exception was thrown 
            //and thus the test should fail.
            bool exception_expected = test_xml.Attribute("exception") != null;

            //Calculate path to output file
            string path_generated_code = Path.ChangeExtension(Path.GetFileName(file_path), ".cs");

            //Call code generator
            ProcessStartInfo start_info = new ProcessStartInfo();
            start_info.FileName = "python";
            start_info.Arguments = string.Format("../../../python_sccd_compiler/sccdc.py {0} -o {1} -p threads -l C#", file_path, path_generated_code);
            start_info.UseShellExecute = false;
            start_info.RedirectStandardOutput = true;
            //Print any output the compiler gave
            using (Process process = Process.Start(start_info))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }

            //Check if file exists
            bool target_file_exists = File.Exists(path_generated_code);
            if (exception_expected){
                Assert.AreEqual(false, target_file_exists, "An exception was expected to be thrown by the compiler but the SCCD compiler completed successfully.");
                return; //No target file as expected, we can end the test.
            } 
            else{
                Assert.AreEqual(true, target_file_exists, "The SCCD Compiler did not complete compilation. No generated file has been found.");
            }

            //Compile generated code
            CodeCompiler code_compiler = new CodeCompiler();
            code_compiler.AddReferencedAssembly("System.dll");
            code_compiler.AddReferencedAssembly("sccdlib.dll");
            Assembly assembly = code_compiler.compileFile(path_generated_code);
            Type class_type = assembly.GetType("Controller");
            //Execute model
            dynamic controller = Activator.CreateInstance(class_type, new object [] {false});
            controller.start();
            controller.join();
        }

        private static IEnumerable GetTestCases()
        {
            foreach (string file_path in Directory.EnumerateFiles("../../../test_files"))
            {
                yield return new TestCaseData(file_path).SetName(Path.GetFileNameWithoutExtension(file_path));
            }
        }
    }
}

