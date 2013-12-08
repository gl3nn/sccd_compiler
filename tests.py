from unittest import TestCase, main
import SCCDC
import importlib
import os
from python_generator import PythonGenerator
from python_runtime.statecharts_core import Event, OutputListener

#http://docs.python.org/2/library/unittest.html

TEST_FILES_FOLDER = "test_files"

class TestSequenceFunctions(TestCase):
    def setUp(self):
        self.controller = None
        self.generated_file = None
        self.output_listener = None
        self.expected_output = None
        self.delete_generated_file = False
        
    def checkOutput(self):
        if(self.output_listener):
            for count, entry in enumerate(self.expected_output, start=1) :
                port = entry[0]
                event = entry[1]       
                output_event = self.output_listener.fetch(0)
                self.assertNotEqual(output_event, None, "Output event " + str(count) + " is None.")
                self.assertEqual(output_event.getPort(), port, "Ports of output event " + str(count) + " did not match. Expected " + port + ", got " + output_event.getPort() + ".")
                self.assertEqual(output_event.getName(), event, "Names of output event " + str(count) + " did not match. Expected " + event + ", got " + output_event.getName() + ".")
                if len(entry) > 2 :
                    expected_parameters = entry[2:]
                    received_parameters = output_event.getParameters()
                    self.assertLessEqual(len(expected_parameters), len(received_parameters), "Parameters of output event" + str(count) + " did not match. Expected " + str(len(expected_parameters)) + " parameters but only got " + str(len(received_parameters)) + ".")
                    for pindex,p in enumerate(expected_parameters) :
                        self.assertEqual(p, received_parameters[pindex], "Parameter " + str(pindex+1) + " of output event " + str(count) + " did not match. Expected " + str(p) + ", got " + str(received_parameters[pindex]) + ".")
                
            #check if there are no extra events          
            self.assertEqual(self.output_listener.fetch(0), None, "More output events than expected.")    
               
    def tearDown(self):
        self.controller = None
        if self.generated_file and self.delete_generated_file :
            os.remove(self.generated_file)
            os.remove(self.generated_file + "c")
            self.generated_file = None
        
    def generate(self, source_file):
        abstract_class_diagram  = SCCDC.process(TEST_FILES_FOLDER + "/" + source_file + ".xml")
        self.generated_file = source_file + ".py"
        self.delete_generated_file = not os.path.isfile(self.generated_file)
        PythonGenerator(abstract_class_diagram, self.generated_file).generate()
        import_file = importlib.import_module(source_file)
        self.controller = import_file.Controller(None, False)
        
    def expect(self, expected):                
        ports = set([entry[0] for entry in expected])
        if not ports :
            print "Invalid expected eventslist."
            return
        self.output_listener = OutputListener(list(ports))
        self.expected_output = expected
        self.controller.addOutputListener(self.output_listener)
        self.controller.start()
        self.controller.join()
        self.checkOutput()
            
               
    def test_after(self) :
        self.generate("test_after")
        self.expect([
            ("test_output", "in_state_2")
        ])
        
    def test_history(self) :
        self.generate("test_history")
        self.controller.addAbsoluteEventList([
            Event("to_state_2", 0.0, "test_input", []),
            Event("to_state_3", 0.0, "test_input", [])                                          
        ])
        self.expect([
            ("test_output", "in_state_1"),
            ("test_output", "in_state_2"),
            ("test_output", "in_state_3"),
            ("test_output", "in_state_2")
        ])
        
    def test_parallel(self):
        self.generate("test_parallel")
        self.controller.addAbsoluteEventList([
            Event("to_state_2", 0.0, "test_input", []),
            Event("to_state_4", 0.0, "test_input", []),
            Event("to_state_1", 0.0, "test_input", []),
            Event("to_state_2", 0.0, "test_input", []),
            Event("to_state_3", 0.0, "test_input", []),
                                                      
        ])
        self.expect([
            ("test_output", "in_state_1"),
            ("test_output", "in_state_3"),
            ("test_output", "in_state_2"),
            ("test_output", "in_state_4"),
            ("test_output", "in_state_1"),
            ("test_output", "in_state_2"),
            ("test_output", "in_state_3")
        ])
        
    def test_object_manager(self):
        self.generate("test_object_manager")
        self.controller.addAbsoluteEventList([
            Event("create", 0.0, "test_input", [])                                               
        ])
        self.expect([
            ("test_output", "request_send"),
            ("test_output", "associate_added"),
            ("test_output", "second_working")
        ])
    
    def test_guard(self):
        self.generate("test_guard")
        self.expect([
            ("test_output", "received", 3)
        ])
        
    def test_instate(self):
        self.generate("test_instate")
        self.expect([
            ("test_output", "check1"),
            ("test_output", "check2"),
            ("test_output", "check3")      
        ])
        
    def test_enter_exit_hierarchy(self):
        self.generate("test_enter_exit_hierarchy")
        self.controller.addAbsoluteEventList([
            Event("to_composite", 0.0, "test_input", []),
            Event("to_inner2", 0.0, "test_input", []),
            Event("to_outside", 0.0, "test_input", []),
            Event("to_inner3", 0.0, "test_input", []),
            Event("to_outside", 0.0, "test_input", []),
            Event("to_inner4", 0.0, "test_input", []),                                        
        ])
        self.expect([
            ("test_output", "enter_state1"),
            ("test_output", "enter_inner1"),
            ("test_output", "exit_inner1"),
            ("test_output", "enter_inner2"),
            ("test_output", "exit_inner2"),
            ("test_output", "exit_state1"),
            ("test_output", "enter_state2"),
            ("test_output", "enter_inner3"),
            ("test_output", "exit_inner3"),
            ("test_output", "exit_state2"),
            ("test_output", "enter_state2"),
            ("test_output", "enter_inner4"),
        ])
        
    def test_parallel_history(self):
        self.generate("test_parallel_history")
        self.controller.addAbsoluteEventList([
            Event("to_state_2", 0.0, "test_input", []),
            Event("to_state_4", 0.0, "test_input", []),
            Event("to_outer_1", 0.0, "test_input", []),
            Event("to_outer_2", 0.0, "test_input", []),
            Event("to_history_1", 0.0, "test_input", []),
            Event("to_history_2", 0.0, "test_input", []),                                        
        ])
        self.expect([
            ("test_output", "in_state_1"),
            ("test_output", "in_state_3"),
            ("test_output", "in_state_2"),
            ("test_output", "in_state_4"),
            ("test_output", "in_outer_1"),
            ("test_output", "in_outer_2"),
            ("test_output", "in_state_2"),
            ("test_output", "in_state_4")
        ])
        
    def test_history_deep(self):
        self.generate("test_history_deep")
        self.expect([
            ("test_output", "check1"),
            ("test_output", "check2"),
            ("test_output", "check3")
        ])
        
    def test_history_parallel_deep(self):
        self.generate("test_history_parallel_deep")
        self.expect([
            ("test_output", "check1"),
            ("test_output", "check2"),
            ("test_output", "check3")
        ])
        
    def test_fault_duplicate_state_id(self):
        with self.assertRaises(SCCDC.CompilerException):
            self.generate("test_fault_duplicate_state_id")
    
    def test_correct_duplicate_state_id(self):
        self.generate("test_correct_duplicate_state_id")
        
    def test_outer_first(self):
        self.generate("test_outer_first")
        self.controller.addAbsoluteEventList([
            Event("event", 0.0, "test_input", [])                                      
        ])
        self.expect([
            ("test_output", "in_b")
        ])
        
    def test_inner_first(self):
        self.generate("test_inner_first")
        self.controller.addAbsoluteEventList([
            Event("event", 0.0, "test_input", [])                                      
        ])
        self.expect([
            ("test_output", "in_a")
        ])
        
    def test_fault_multiple_unconditional(self):
        with self.assertRaises(SCCDC.TransitionException):
            self.generate("test_fault_multiple_unconditional")
    
if __name__ == '__main__':
    main()
