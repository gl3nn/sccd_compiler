class Logger(object):
    verbose = 0 #-1= no output
                #0 = only errors
                #1 = only warnings and errors
                #2 = all output
                
    @staticmethod   
    def showError(error):
        if(Logger.verbose > -1) :
            print "ERROR : " + error
                
    @staticmethod
    def showWarning(warning):
        if(Logger.verbose > 0) :
            print "WARNING : " + warning
            
    @staticmethod    
    def showInfo(info):
        if(Logger.verbose > 1) :
            print "INFO : " + info

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