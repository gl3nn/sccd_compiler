from visitor import Visitor
from utils import FileOutputer
from utils import Enum
from utils import Logger
from compiler_exceptions import CodeBlockException

NOT_SET = 0
SPACES_USED = 1
TABS_USED = 2

Languages = Enum("Python","CSharp") 
Platforms = Enum("Threads","GameLoop") 

class CodeGenerator(Visitor):
    def __init__(self):
        self.supported_platforms = []
        
    def generate(self, class_diagram, output_file, platform):
        self.platform = platform
        if self.platform not in self.supported_platforms :
            Logger.showError("Unsupported platform.")
            return False
        try :
            self.fOut = FileOutputer(output_file)
            class_diagram.accept(self)
        finally :
            self.fOut.close()
        return True
    
    def writeCodeCorrectIndent(self, body):
        lines = body.split('\n')
        while( len(lines) > 0 and lines[-1].strip() == "") :
            del(lines[-1])
    
        index = 0;
        while( len(lines) > index and lines[index].strip() == "") :       
            index += 1
            
        if index >= len(lines) :
            return
        #first index where valid code is present
        to_strip_index = len(lines[index].rstrip()) - len(lines[index].strip()) 
        indent_type = NOT_SET;
            
        while index < len(lines):
            strip_part = lines[index][:to_strip_index]
            
            if( ('\t' in strip_part and ' ' in strip_part) or
                (indent_type == SPACES_USED and '\t' in strip_part) or
                (indent_type == TABS_USED and ' ' in strip_part)
            ) :
                raise CodeBlockException("Mixed tab and space indentation!")
            
            if indent_type == NOT_SET :
                if ' ' in strip_part :
                    indent_type = SPACES_USED
                elif '\t' in strip_part :
                    indent_type = TABS_USED
                    
            self.fOut.write(lines[index][to_strip_index:])
            index += 1