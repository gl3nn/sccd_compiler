using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using sccdlib;

namespace csharp_tests
{
    public abstract class TestsBase
    {
        protected string path_generated_code;
        protected bool keep_after_test;

        protected abstract bool generate(string file_path, string expected_exception);

        [Test, TestCaseSource(typeof(TestsBase),"GetTestCases")]
        public void testXML(string file_path)
        {
            XElement test_xml = XDocument.Load(file_path).Root.Element("test");
            Assert.AreNotEqual(null, test_xml, "No test data found. (A test that should just compile correctly, still needs an empthy test tag.)");

            //Calculate path to output file
            this.path_generated_code = Path.ChangeExtension(Path.GetFileName(file_path), ".cs");

            this.keep_after_test = false;
            if (File.Exists(this.path_generated_code))
            {
                this.keep_after_test = true;
                File.Delete(this.path_generated_code);
            }

            string expected_exception = null;
            if (test_xml.Attribute("exception") != null && test_xml.Attribute("exception").Value.Trim() != "")
            {
                expected_exception = test_xml.Attribute("exception").Value.Trim();
            }

            //Call code generator
            if (!this.generate(file_path, expected_exception))
                return;

            //Compile generated code
            CodeCompiler code_compiler = new CodeCompiler();
            code_compiler.AddReferencedAssembly("System.dll");
            code_compiler.AddReferencedAssembly("sccdlib.dll");
            Assembly assembly = code_compiler.compileFile(this.path_generated_code);
            Type class_type = assembly.GetType("Controller");

            //Prepare expected output
            XElement expected_xml = test_xml.Element("expected");
            if (expected_xml == null)
                return;

            HashSet<string> output_ports = new HashSet<string>();
            List<List<TestEvent>> expected_result = new List<List<TestEvent>>();

            foreach (XElement slot_xml in expected_xml.Elements("slot"))
            {
                List<TestEvent> slot = new List<TestEvent>();
                foreach (XElement event_xml in slot_xml.Elements("event"))
                {
                    string event_name = event_xml.Attribute("name").Value;
                    string port = event_xml.Attribute("port").Value;
                    List<string> parameters = new List<string>();
                    foreach (XElement parameter_xml in event_xml.Elements("parameter"))
                    {
                        string parameter_value = parameter_xml.Attribute("value").Value;
                        parameters.Add(parameter_value);
                    }
                    slot.Add(new TestEvent(event_name, port, parameters));
                    output_ports.Add(port);
                }
                if (slot.Count > 0)
                    expected_result.Add(slot);
            }

            //Prepare model
            ThreadsControllerBase controller = (ThreadsControllerBase) Activator.CreateInstance(class_type, new object [] {false});
            IOutputListener output_listener = controller.addOutputListener(output_ports.ToArray());

            //Input
            XElement input_xml = test_xml.Element("input");
            if (input_xml != null)
            {
                foreach (XElement event_xml in input_xml.Elements("event"))
                {
                    controller.addInput(new Event(event_xml.Attribute("name").Value, event_xml.Attribute("port").Value), Convert.ToDouble(event_xml.Attribute("time").Value));
                }
            }

            //Execute model
            controller.start();
            controller.join();

            //Check output
            for (int slot_index = 0; slot_index < expected_result.Count; slot_index++)
            {
                List<TestEvent> slot = expected_result[slot_index];
                List<TestEvent> remaining_options = new List<TestEvent>(slot);
                List<Event> received_output = new List<Event>();
                while (remaining_options.Count > 0)
                {
                    Event output_event = output_listener.fetch();
                    Assert.AreNotEqual(null, output_event,
                                       string.Format("Expected results slot {0} mismatch. Expected [{1}], but got [{2}] followed by null instead.", slot_index, string.Join(", ", slot), string.Join(", ", received_output))
                                       );
                    received_output.Add(output_event);
                    int i = 0;
                    foreach (TestEvent option in remaining_options)
                    {
                        if (option.matches(output_event))
                            break;
                        i++;
                    }
                    //Mismath?
                    Assert.AreNotEqual(remaining_options.Count,i,
                                       string.Format("Expected results slot {0} mismatch. Expected [{1}], but got [{2}] instead.", slot_index, string.Join(", ", slot), string.Join(", ", received_output))
                                       );
                    remaining_options.RemoveAt(i);
                }
            }
            //check if there are no extra events          
            Assert.AreEqual(null, output_listener.fetch(), "More output events than expected on selected ports.");
        }

        [TearDown]
        public void cleanup()
        {
            if (!keep_after_test && File.Exists(this.path_generated_code))
            {
                File.Delete(this.path_generated_code);
            }
        }

        protected static IEnumerable GetTestCases()
        {
            foreach (string file_path in Directory.EnumerateFiles("../../../test_files"))
            {
                if (Path.GetExtension(file_path) == ".xml")
                    yield return new TestCaseData(file_path).SetName(Path.GetFileNameWithoutExtension(file_path));
            }
        }
    }
}

