using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Xml.Linq;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using sccdlib;

namespace csharp_tests
{
    [TestFixture]
    public class Main
    {
        string path_generated_code;
        bool keep_after_test;
   
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
            this.path_generated_code = Path.ChangeExtension(Path.GetFileName(file_path), ".cs");

            this.keep_after_test = false;
            if (File.Exists(this.path_generated_code))
            {
                this.keep_after_test = true;
                File.Delete(this.path_generated_code);
            }

            //Call code generator
            ProcessStartInfo start_info = new ProcessStartInfo();
            start_info.FileName = "python";
            start_info.Arguments = string.Format("../../../python_sccd_compiler/sccdc.py {0} -o {1} -p threads -l C# -v 0", file_path, this.path_generated_code);
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
            bool target_file_exists = File.Exists(this.path_generated_code);
            if (exception_expected)
            {
                Assert.AreEqual(false, target_file_exists, "An exception was expected to be thrown by the compiler but the SCCD compiler completed successfully.");
                return; //No target file as expected, we can end the test.
            } else
            {
                Assert.AreEqual(true, target_file_exists, "The SCCD Compiler did not complete compilation. No generated file has been found.");
            }

            //Compile generated code
            CodeCompiler code_compiler = new CodeCompiler();
            code_compiler.AddReferencedAssembly("System.dll");
            code_compiler.AddReferencedAssembly("sccdlib.dll");
            Assembly assembly = code_compiler.compileFile(this.path_generated_code);
            Type class_type = assembly.GetType("Controller");
            //Execute model
            ThreadsControllerBase controller = (ThreadsControllerBase)Activator.CreateInstance(class_type, new object [] {false});
            controller.addOutputListener(new string[]{"test_output"});

            XElement input_xml = test_xml.Element("input");
            if (input_xml != null)
            {
                foreach (XElement event_xml in input_xml.Elements("event"))
                {
                    controller.addInput( new Event(event_xml.Attribute("name").ToString(),event_xml.Attribute("port").ToString()) );
                }
            }

            XElement expected_xml = test_xml.Element("expected");
            if (expected_xml == null)
                return;

            controller.start();
            controller.join();
        }

        [TearDown]
        public void cleanup()
        {
            if (!keep_after_test && File.Exists(this.path_generated_code))
            {
                File.Delete(this.path_generated_code);
            }
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

