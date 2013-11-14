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

##################################    
    
class StringException(Exception):
    def __init__(self, value):
        self.value = value
    def __str__(self):
        return repr(self.value)

##################################
    
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