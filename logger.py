verbose = 0 #0 = no output
            #1 = only warnings
            #2 = all output

def showWarning(warning):
    if(verbose > 0) :
        print "WARNING : " + warning
        
def showInfo(info):
    if(verbose > 1) :
        print "INFO : " + info
        
def showError(error):
    print "ERROR : " + error