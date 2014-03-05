from visitor import Visitor
from constructs import INSTATE_SEQ
from compiler_exceptions import CompilerException

class StateReferenceException(CompilerException):
    pass

class StateLinker(Visitor):
    
    def __init__(self):
        self.visiting_statechart = None
        self.visiting_node = None
    
    def visit_ClassDiagram(self, class_diagram): 
        for c in class_diagram.classes :
            c.accept(self)

    def visit_Class(self, c):
        c.statechart.accept(self)
        
    def visit_StateChart(self, statechart):
        self.visiting_statechart = statechart
        for node in statechart.basics + statechart.composites:
            node.accept(self)
                     
    def visit_StateChartNode(self, node):
        self.visiting_node = node
        node.entry_action.accept(self)
        node.exit_action.accept(self)
        for transition in node.transitions :
            transition.accept(self)
            
    def visit_StateChartTransition(self, transition):
        try :
            transition.target.accept(self)
        except StateReferenceException as exception :
            raise StateReferenceException("Transition from <" + self.visiting_node.getFullID() + "> has invalid target. " + exception.message)
        try :
            transition.action.accept(self)
        except StateReferenceException as exception :
            raise StateReferenceException("Transition from <" + self.visiting_node.getFullID() + "> has invalid action. " + exception.message)
        try :
            if transition.guard :
                transition.guard.accept(self)
        except StateReferenceException as exception :
            raise StateReferenceException("Transition from <" + self.visiting_node.getFullID() + "> has invalid guard. " + exception.message)
        
    def visit_StatePath(self, state_path):
        path_string = state_path.path_string
        current_node = self.visiting_node #will be used to traverse the path
        
        if len(path_string) == 0 :
            raise StateReferenceException("StatePath expects non-empty string.")
        shift = 0
        if path_string[0] == "/" :
            current_node = self.visiting_statechart.root
            shift = 1
        while True :
            if path_string[shift:shift+3] == "../":
                current_node = current_node.getParentNode()
                if current_node == None :
                    raise StateReferenceException("Illegal use of parent operator. Root of statechart reached.")
                shift += 3
            elif shift == len(path_string) -1 and path_string[shift] == "." :
                shift += 1   
            elif path_string[shift:shift+2] == "./":
                shift += 2
            else :
                break
        path_string = path_string[shift:]
        if path_string != "" :
            path =  path_string.split("/")
        else :
            path = []
            
        for step in path :
            found = False
            for child in current_node.children :
                if child.getName() == step : 
                    found = True
                    current_node = child
                    break
            if not found :
                raise StateReferenceException('Invalid state reference "' + state_path.path_string + '".')
                #TODO : detailed exception
        state_path.target_node = current_node
        
    #edit this class out
    def visit_EnterExitAction(self, action):
        if action.action :
            action.action.accept(self)
            
    def visit_Action(self, action):
        for subaction in action.sub_actions :
            subaction.accept(self)
            
    def visit_InStateCall(self, call):
        try :
            call.target.accept(self)
        except StateReferenceException :
            raise StateReferenceException("Invalid state reference for " + INSTATE_SEQ + " call.")
        
    def visit_Assign(self, assign):
        assign.expression.accept(self)