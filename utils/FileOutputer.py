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