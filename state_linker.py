from visitor import Visitor
from constructs import INSTATE_SEQ
from compiler_exceptions import CompilerException
from lexer import Lexer, TokenType

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
        
    def visit_StateReference(self, state_path):
        current_node = None #Will be used to find the target state(s)
        split_stack = [] #used for branching

        lx = Lexer()
        lx.input(state_path.path_string)
        token = None
        last_token = None

        while True :
            last_token = token
            token = lx.nextToken() #get next token
            if token == None : #if that next token is None, then we ran out of tokens and break
                break
            
            if current_node == None : #current_node is not set yet or has been reset, the CHILD token can now have a special meaning
                if token.type == TokenType.SLASH :
                    #Root detected
                    current_node = self.visiting_statechart.root
                    #Token consumed so continue
                    continue
                else :
                    current_node = self.visiting_node
                    
            
            if token.type == TokenType.PARENT :
                current_node = current_node.getParentNode()
                if current_node == None :
                    raise StateReferenceException("Illegal use of parent operator at position " + str(token.pos) + " in state reference. Root of statechart reached.")
            elif token.type == TokenType.CURRENT :
                continue
            elif token.type == TokenType.SLASH :
                pass
            elif token.type == TokenType.WORD :
                #try to advance to next child state
                cname = token.val
                found = False
                for child in current_node.children :
                    if child.getName() == cname : 
                        found = True
                        current_node = child
                        break
                if not found :
                    raise StateReferenceException("Refering to non exiting node at posisition " + str(token.pos) + " in state reference.")
            elif token.type == TokenType.LB :
                split_stack.append(current_node)
            elif token.type == TokenType.RB :
                split_stack.pop()
            elif token.type == TokenType.COMMA :
                state_path.target_states.append(current_node)
                if len(split_stack) > 0 :
                    current_node = split_stack[-1]
                else :
                    current_node = None
            
            else :
                raise StateReferenceException("Invalid token at position " + str(token.pos) + ".")

            
        #TODO : better validation of the target! When is it a valid state configuration?
        
        if (len(split_stack) != 0) or (current_node is None) : #RB missing or extra COMMA
            raise StateReferenceException("Missing token at position " + str(last_token.pos) + ".")
        
        state_path.target_states.append(current_node)
            
        if len(state_path.target_states) == 0 :
            raise StateReferenceException("Meaningless state reference.")
        

        

        
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