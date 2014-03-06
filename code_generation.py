from visitor import Visitor
from utils import FileOutputer
from utils import Enum
from utils import Logger

Languages = Enum("Python","CSharp") 
Protocols = Enum("Threads","GameLoop") 

class CodeGenerator(Visitor):
    def __init__(self, class_diagram, output_file, protocol):
        self.output_file = output_file
        self.class_diagram = class_diagram
        self.protocol = protocol
        self.supported_protocols = []
        
    def generate(self):
        if self.protocol not in self.supported_protocols :
            Logger.showError("Unsupported protocol.")
            return
        try :
            self.fOut = FileOutputer(self.output_file)
            self.class_diagram.accept(self)
        finally :
            self.fOut.close()