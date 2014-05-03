using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace csharp_tests
{
    [TestFixture]
    [Category("Python")]
    public class PythonTests : TestsBase
    {

        protected override bool generate(string file_path, string expected_exception)
        {
            ProcessStartInfo start_info = new ProcessStartInfo();
            start_info.FileName = "python";
            start_info.Arguments = string.Format("../../../python_sccd_compiler/sccdc.py {0} -o {1} -p threads -l C# -v -1", file_path, this.path_generated_code);
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
            //Exception check
            //If the test file expects an exception to be thrown by the compiler,
            //we just try to compile the model and see if a file got generated.
            //If the result file got generated, this means that no exception was thrown
            //and thus the test should fail.
            bool target_file_exists = File.Exists(this.path_generated_code);
            if (expected_exception != null)
            {
                Assert.AreEqual(false, target_file_exists, "An exception was expected to be thrown by the compiler but the SCCD compiler completed successfully.");
                return false; //No target file as expected, we can end the test.
            }
            Assert.AreEqual(true, target_file_exists, "The SCCD Compiler did not complete compilation. No generated file has been found.");
            return true;
        }
    }
}

