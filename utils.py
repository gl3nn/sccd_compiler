class Logger(object):
    verbose = 0 #0 = no output
                #1 = only warnings
                #2 = all output
                
    @staticmethod
    def showWarning(self, warning):
        if(self.verbose > 0) :
            print "WARNING : " + warning
            
    @staticmethod    
    def showInfo(self, info):
        if(self.verbose > 1) :
            print "INFO : " + info
    @staticmethod   
    def showError(self, error):
        print "ERROR : " + error

#######################

class Enum():    
    def __init__(self, *entries):
        self._keys = entries
        self._map = {}
        for v,k in enumerate(self._keys) :
            self._map[k] = v
            
    def __getattr__(self, name):
        return self._map[name]
            
    def name_of(self, index):
        return self._keys[index]

#######################

class FileOutputer:

    def __init__(self, filename):
        self.out = open(filename, 'w')
        self.indentLevel = 0
        self.indentSpace = "    "
        self.first_write = True

    def write(self, text = ""):
        if self.first_write :
            self.first_write = False
            if text == "":
                self.out.write(self.indentLevel*self.indentSpace)
            else:
                self.out.write(self.indentLevel*self.indentSpace + text)  
        else:
            if text == "":
                self.out.write("\n" + self.indentLevel*self.indentSpace)
            else:
                self.out.write("\n" + self.indentLevel*self.indentSpace + text)
    
    def extendWrite(self, text = ""):
        self.out.write(text)            
                
    def indent(self):
        self.indentLevel+=1

    def dedent(self):
        self.indentLevel-=1

    def close(self):
        self.out.close()


#######################

from compiler_exceptions import CompilerException

class StringException(CompilerException):
    def __init__(self, value):
        self.value = value
    def __str__(self):
        return repr(self.value)

class StringCode:
    def __init__(self):
        self.string = ""
        self.indentLevel = 0
        self.indentSpace = "    "
        
    def addLine(self, text=""):
        self.string += self.indentLevel*self.indentSpace + text + "\n"
        
    def addBlock(self, block):
        lines = splitToLines(block)
        for line in lines:
            self.string += self.indentLevel*self.indentSpace + line + "\n"
        
    def indent(self):
        self.indentLevel+=1

    def dedent(self):
        self.indentLevel-=1
        
    def getString(self):
        return self.string
    
def listToString(input_list):
    return '[' + ", ".join(input_list) + ']'

def splitToLines(body):
    lines = body.split('\n')
    if(lines[-1].strip() == ""):
        del(lines[-1])
    return lines

def writeCode(body, fOut):
    lines = splitToLines(body);
    for line in lines:
        fOut.write(line)
        
def writeCodeCorrectIndent(body, fOut):
    lines = splitToLines(body)
    if lines == [] :
        return
    if lines[0].strip() == '' :
        del lines[0]
    to_strip = len(lines[0]) - len(lines[0].strip()) 
    for line in lines:
        if('\t' in line and '  ' in line) :
            raise StringException("Mixed tab and space indentation!")
        fOut.write(line[to_strip:])