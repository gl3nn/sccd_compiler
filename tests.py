from unittest import TestCase, main
import sccdc
import importlib
import os
from compiler_exceptions import CompilerException, TransitionException
from code_generation import Protocols
from python_generator import PythonGenerator
from python_runtime.statecharts_core import Event

#http://docs.python.org/2/library/unittest.html

TEST_FILES_FOLDER = "test_files"


class TestEvent(object):
    def __init__(self, name, port, parameters = []):
        self.name = name
        self.port = port
        self.parameters = parameters
        
    def matches(self, event):
        if event is None :
            return False
        if event.getName() != self.name :
            return False
        if event.getPort() != self.port :
            return False
        compare_parameters = event.getParameters()
        if len(self.parameters) != len(compare_parameters) :
            return False
        for index in xrange(len(self.parameters)) :
            if self.parameters[index] !=  compare_parameters[index]:
                return False
        return True
    
    def __repr__(self):
        representation = "(event name : " + str(self.name) + "; port : " + str(self.port)
        if self.parameters :
            representation += "; parameters : " + str(self.parameters)
        representation += ")"
        return representation

class TestSequenceFunctions(TestCase):
    def setUp(self):
        self.controller = None
        self.generated_file = None
        self.output_listener = None
        self.expected_output = None
        self.delete_generated_file = False
        
        
        
    def checkOutput(self):
        if(self.output_listener):
            for (entry_index, expected_entry) in enumerate(self.expected_output, start=1) :
                
                all_options = []
                if isinstance(expected_entry, tuple) :
                    all_options.append(TestEvent(expected_entry[1],expected_entry[0],expected_entry[2:]))
                else :
                    for expected_event in expected_entry :
                        all_options.append(TestEvent(expected_event[1],expected_event[0],expected_event[2:]))                    
                     
                remaining_options = all_options[:]
                received_output = [] 
                while remaining_options :
                    output_event = self.output_listener.fetch()
                    received_output.append(output_event)
                    match_index = -1
                    for (index, option) in enumerate(remaining_options) :
                        if option.matches(output_event) :
                            match_index = index
                            break
                    
                    self.assertNotEqual(match_index, -1, "Expected results entry " + str(entry_index) + " mismatch. Expected " + str(all_options) + ", but got " + str(received_output) +  " instead.") #no match found in the options
                    remaining_options.pop(match_index)
                                
            #check if there are no extra events          
            self.assertEqual(self.output_listener.fetch(0), None, "More output events than expected.")    
               
    def tearDown(self):
        self.controller = None
        if self.generated_file and self.delete_generated_file :
            os.remove(self.generated_file)
            os.remove(self.generated_file + "c")
            self.generated_file = None
        
    def generate(self, source_file):
        abstract_class_diagram  = sccdc.createAST(TEST_FILES_FOLDER + "/" + source_file + ".xml")
        self.generated_file = source_file + ".py"
        self.delete_generated_file = not os.path.isfile(self.generated_file)
        PythonGenerator(abstract_class_diagram, self.generated_file, Protocols.Threads).generate()
        import_file = importlib.import_module(source_file)
        self.controller = import_file.Controller(False)
        
    def expect(self, expected):                
        ports = set([entry[0] for entry in expected])
        if not ports :
            print "Invalid expected eventslist."
            return
        self.expected_output = expected
        self.output_listener = self.controller.addOutputListener(list(ports))
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
        self.controller.addEventList([
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
        self.controller.addEventList([
            Event("to_state_2", 0.0, "test_input", []),
            Event("to_state_4", 0.0, "test_input", []),
            Event("to_state_1", 0.0, "test_input", []),
            Event("to_state_2", 0.0, "test_input", []),
            Event("to_state_3", 0.0, "test_input", []),
                                                      
        ])
        self.expect([
            [("test_output", "in_state_1"),("test_output", "in_state_3")],
            ("test_output", "in_state_2"),
            ("test_output", "in_state_4"),
            ("test_output", "in_state_1"),
            ("test_output", "in_state_2"),
            ("test_output", "in_state_3")
        ])
        
    def test_object_manager(self):
        self.generate("test_object_manager")
        self.controller.addEventList([
            Event("create", 0.0, "test_input", [])                                               
        ])
        self.expect([
            ("test_output", "request_send"),
            ("test_output", "instance_created"),
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
        self.controller.addEventList([
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
        self.controller.addEventList([
            Event("to_state_2", 0.0, "test_input", []),
            Event("to_state_4", 0.0, "test_input", []),
            Event("to_outer_1", 0.0, "test_input", []),
            Event("to_outer_2", 0.0, "test_input", []),
            Event("to_history_1", 0.0, "test_input", []),
            Event("to_history_2", 0.0, "test_input", []),                                        
        ])
        self.expect([
            [("test_output", "in_state_1"),("test_output", "in_state_3")],
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
        with self.assertRaises(CompilerException):
            self.generate("test_fault_duplicate_state_id")
    
    def test_correct_duplicate_state_id(self):
        self.generate("test_correct_duplicate_state_id")
        
    def test_outer_first(self):
        self.generate("test_outer_first")
        self.controller.addEventList([
            Event("event", 0.0, "test_input", [])                                      
        ])
        self.expect([
            ("test_output", "in_b")
        ])
        
    def test_inner_first(self):
        self.generate("test_inner_first")
        self.controller.addEventList([
            Event("event", 0.0, "test_input", [])                                      
        ])
        self.expect([
            ("test_output", "in_a")
        ])
        
    def test_fault_multiple_unconditional(self):
        with self.assertRaises(TransitionException):
            self.generate("test_fault_multiple_unconditional")
    
if __name__ == '__main__':
    main()
