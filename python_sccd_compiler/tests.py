import unittest
import sccdc
import importlib
import os
import xml.etree.ElementTree as ET
from compiler_exceptions import CompilerException, TransitionException
from code_generation import Platforms, Languages
from python_runtime.statecharts_core import Event

SHARED_TEST_FILES_FOLDER = "../test_files"

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
            if self.parameters[index] !=  str(compare_parameters[index]):
                return False
        return True
    
    def __repr__(self):
        representation = "(event name : " + str(self.name) + "; port : " + str(self.port)
        if self.parameters :
            representation += "; parameters : " + str(self.parameters)
        representation += ")"
        return representation

class XMLTestCase(unittest.TestCase):
    
    def __init__(self, file_name):
        super(XMLTestCase, self).__init__()
        self.file_name = file_name
        self.name = os.path.splitext(file_name)[0]
        self.source_path = os.getcwd() + "/" + SHARED_TEST_FILES_FOLDER + "/" + self.file_name
        self.target_path = os.getcwd() + "/" + self.name + ".py"
        self.file_generated = False
                    
    def __str__(self):
        return self.file_name
        
    def setUp(self):
        self.delete_generated_file = not os.path.isfile(self.target_path)
        
    def tearDown(self):
        if self.file_generated and self.delete_generated_file :
            os.remove(self.target_path)
            os.remove(self.target_path + "c")
            

    def runTest(self):
        test_xml = ET.parse(self.source_path).getroot().find("test")
        self.assertIsNot(test_xml, None, "No test data found. (A test that should just compile correctly, still needs an empthy test tag.)")
        
        #Check if the exception attribute is set and act accordingly
        exception_attribute = test_xml.get("exception","")
        if exception_attribute == "" :
            sccdc.generate(self.source_path, self.target_path, Languages.Python, Platforms.Threads)
        else :
            if exception_attribute == "CompilerException" :
                with self.assertRaises(CompilerException):
                    sccdc.generate(self.source_path, self.target_path, Languages.Python, Platforms.Threads)
            elif exception_attribute == "TransitionException" :
                with self.assertRaises(TransitionException):
                    sccdc.generate(self.source_path, self.target_path, Languages.Python, Platforms.Threads)
            else :
                raise AssertionError("Invalid value for the exception attribute.")
            return
            
        
        self.file_generated = True
        import_file = importlib.import_module(self.name)
        self.controller = import_file.Controller(False)
        
        #Preparing input for controller
        input_xml = test_xml.find("input")
        if input_xml is not None :
            for event_xml in input_xml :
                if event_xml.tag == "event" :
                    self.controller.addInput(Event(event_xml.get("name"), event_xml.get("port")))
                    
        expected_xml = test_xml.find("expected")
        if expected_xml is None : 
            #no expected result, so we just simulate without catching output
            self.controller.start()
            self.controller.join()
            return
        
        #Creating a datastructure for the expected output
        expected_result = []
        output_ports = set()

        for slot_xml in expected_xml :
            if slot_xml.tag == "slot" :
                slot = []
                for event_xml in slot_xml :
                    if event_xml.tag == "event" :
                        event_name = event_xml.get("name")
                        port = event_xml.get("port")
                        parameters = []
                        parameters_xml = event_xml.findall("parameter")
                        for parameter_xml in parameters_xml :
                            parameter_value = parameter_xml.get("value", None)
                            if parameter_value is not None :
                                parameters.append(parameter_value)
                        slot.append(TestEvent(event_name, port,parameters))
                        output_ports.add(port)
                if slot :
                    expected_result.append(slot)

        #Execution
        output_listener = self.controller.addOutputListener(list(output_ports))
        self.controller.start()
        self.controller.join()
        
        #Check output
        for (slot_index, slot) in enumerate(expected_result, start=1) : 
            remaining_options = slot[:]
            received_output = [] 
            while remaining_options :
                output_event = output_listener.fetch()
                received_output.append(output_event)
                match_index = -1
                for (index, option) in enumerate(remaining_options) :
                    if option.matches(output_event) :
                        match_index = index
                        break
                
                self.assertNotEqual(match_index, -1, "Expected results slot " + str(slot_index) + " mismatch. Expected " + str(slot) + ", but got " + str(received_output) +  " instead.") #no match found in the options
                remaining_options.pop(match_index)
                            
        #check if there are no extra events          
        self.assertEqual(output_listener.fetch(0), None, "More output events than expected on selected ports.")   
        
        
if __name__ == '__main__':
    suite = unittest.TestSuite()

    for file_name in os.listdir(os.getcwd() + "/" + SHARED_TEST_FILES_FOLDER):
        if file_name.endswith(".xml"): 
            suite.addTest(XMLTestCase(file_name))
            
    unittest.TextTestRunner(verbosity=2).run(suite)
