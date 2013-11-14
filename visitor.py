from utils.FileOutputer import FileOutputer

class Visitor(object):
    def _visit(self, node, prepend, *args):
        prepend = prepend + "_"
        meth = None
        for cls in node.__class__.__mro__:
            meth_name = prepend + cls.__name__
            meth = getattr(self, meth_name, None)
            if meth:
                break

        if not meth:
            meth = self.generic_visit
        return meth(node, *args)
    
    def visit(self, node, *args):
        self._visit(node, "visit", *args)
    
    def enter(self, node, *args):
        self._visit(node, "enter", *args)
        
    def exit(self, node, *args):
        self._visit(node, "exit", *args)

    def generic_visit(self, node):
        print 'generic_visit '+node.__class__.__name__
        
class Visitable(object):
    def accept(self, visitor):
        visitor.visit(self)
        
class CodeGenerator(Visitor):
    def __init__(self, class_diagram, output_file):
        self.output_file = output_file
        self.class_diagram = class_diagram
        
    def generate(self):
        try :
            self.fOut = FileOutputer(self.output_file)
            self.class_diagram.accept(self)
        finally :
            self.fOut.close()
    