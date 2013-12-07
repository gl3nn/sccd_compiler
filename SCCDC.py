import xml.etree.ElementTree as ET
import os
import abc
import re
import python_generator as Python
from visitor import Visitable


# http://docs.python.org/2/library/xml.etree.elementtree.html

# list of reserved words
reserved = ["__init__", "__del__", "init", "transition", "microstep", "step", "instate", "event", 
            "broadcast", "getEarliestEvent", "__str__", "controller", "currentTime", 
            "currentState", "loopMax", "timers", "eventQueue", "loopCount", "stateChanged", "historyState",
            "root", "narrowcast"]

SELF_REFERENCE_SEQ = 'SELF'
INSTATE_SEQ = 'INSTATE'

##################################

class Target(Visitable):
    def __init__(self, input_string, source_node):
        #source_node = source_node.getParentNode()
        self.input_string = input_string
        self.statechart = source_node.getParentStateChart()
        self.original_source = source_node
        self.source = source_node
        if len(input_string) == 0 :
            raise CompilerException("Target node expects non-empty string.")
        shift = 0
        if input_string[0] == "/" :
            self.source = self.statechart.root
            shift = 1
        while True :
            if input_string[shift:shift+3] == "../":
                self.source = self.source.getParentNode()
                if self.source == None :
                    raise CompilerException("Illegal use of parent operator. Root of statechart reached.")
                shift += 3
            elif shift == len(input_string) -1 and input_string[shift] == "." :
                shift +=1   
            elif input_string[shift:shift+2] == "./":
                shift += 2
            else :
                break
        path_string = input_string[shift:]
        if path_string != "" :
            self.path =  path_string.split("/")
        else :
            self.path = []
        self.target_node = None #added when validation of the target is done
    
    def validate(self):
        parent = self.source
        for step in self.path :
            found = False
            for child in parent.children :
                if child.getName() == step : 
                    found = True
                    parent = child
                    break
            if not found :
                raise CompilerException('Invalid state reference "' + self.input_string + '".')
        self.target_node = parent
        
    def getNode(self):
        return self.target_node
    
##################################

"""
    produces a list of parts where macro's are replaced by objects.
"""
def processString(string, current_node = None):
    pieces = []             
    stack = []
    pointer = 0
    last_end = -1
    skip = False
    for match in re.finditer(r"\b[a-zA-Z_]\w*\b", string) :
        while pointer < match.start() :
            if skip :
                skip = False
                continue
            i = string[pointer]
            if i == "'" or i == '"' :
                if stack and stack[-1] == i :
                    stack.pop()
                else :
                    stack.append(i)
            elif i == "/" : #Escape character! Ignore next char
                skip = True
            pointer += 1
        if not stack :
            processed_string = string[last_end+1:pointer]
            matched_string = string[match.start():match.end()]
            created_object = None
            if matched_string == SELF_REFERENCE_SEQ :
                created_object = SelfReference()
                last_end = match.end()-1
            elif matched_string == INSTATE_SEQ :
                if current_node is None :
                    raise CompilerException(INSTATE_SEQ + " call is not allowed here.")
                pattern = re.compile("\\s*\((?P<quoting>[\"\'])([^\"\']+)(?P=quoting)\)")
                result = pattern.search(string, match.end())
                if result :
                    created_object = InStateCall(result.group(2), current_node)
                    last_end = result.end() - 1
            
            if created_object :    
                if processed_string != "" :
                    pieces.append(BareString(processed_string))
                pieces.append(created_object)
                
        pointer = match.end()
    processed_string = string[last_end+1:]
    if processed_string != "" :
        pieces.append(BareString(processed_string))
    return pieces
   
##################################
class ExpressionPart(Visitable):
    __metaclass__  = abc.ABCMeta
    
    def validate(self):
        pass
    
class BareString(ExpressionPart):
    def __init__(self, string):
        self.string = string
    
class SelfReference(ExpressionPart):        
    pass
    
class InStateCall(ExpressionPart):
    def __init__(self, state_string, current_node):
        if state_string == "" :
            raise CompilerException(INSTATE_SEQ + " call expects a non-empty statenode.")
        self.target = Target(state_string, current_node)
        
    def accept(self, visitor):
        visitor.enter(self)
        self.target.accept(visitor)
        visitor.exit(self)
        
    def validate(self):
        try :
            self.target.validate()
        except CompilerException :
            raise CompilerException("Invalid state reference for " + INSTATE_SEQ + " call.")

##################################
    
class Expression(Visitable):
    def __init__(self, string, current_node):
        self.pieces = processString(string, current_node)
          
    def accept(self, visitor):
        for piece in self.pieces :
            piece.accept(visitor)
            
    def validate(self):
        for piece in self.pieces :
            piece.validate()

class LValue(Expression):
    def __init__(self, string):
        self.pieces = processString(string)
        #do some validation, provide parameters to processString to make the function more efficient
     
##################################     
     
class FormalEventParameter(Visitable):
    def __init__(self, name, type = "", default = ""):
        self.name = name
        self.type = type
        self.default = default
        
    def getName(self):
        return self.name
    
##################################
class TriggerEvent:
    def __init__(self, xml_element):
        self.event = xml_element.get("event", "").strip()
        self.port = xml_element.get("port", "").strip()

        self.is_uc = False;
        self.is_after = False
        self.after_index = -1
        self.after_exp = ""
        self.params = []
        
        if self.event == "" :
            if self.port :
                raise CompilerException("Unconditional event can not have a port.")
            self.is_uc = True
            return
        
        if len(self.event) > 7 and self.event[0:6] == "AFTER(" and self.event[-1] == ")":
            if self.port :
                raise CompilerException("AFTER event can not have a port.")
            self.is_after = True
            self.after_exp = self.event[6:-1]
            return
     
        self.params = []
        parameters = xml_element.findall('parameter')    
        for p in parameters :
            name = p.get("name","")
            if not name :
                raise CompilerException("Parameter without name detected.")
            self.params.append(FormalEventParameter(name, p.get("type",""), p.get("default","")))
            
    def getEvent(self):    
        return self.event
    
    def setEvent(self, event):
        self.event = event
    
    def getParameters(self):    
        return self.params
    
    def getPort(self):
        return self.port
        
    def isUC(self):
        return self.is_uc;
    
    def isAfter(self):
        return self.is_after

    def getAfterExp(self):
        return self.after_exp
    
    def getAfterIndex(self):
        return self.after_index
    
    def setAfterIndex(self, after):
        self.after_index = after
        
##################################

class SubAction(Visitable):
    __metaclass__  = abc.ABCMeta
    
    @abc.abstractmethod
    def check(self):
        pass
    
    @classmethod
    def create(cls, xml_element, current_node):
        for subcls in cls.__subclasses__():
            tag = xml_element.tag.lower()
            if subcls.check(tag):
                if tag == Assign.tag or tag == RaiseEvent.tag:
                    return subcls(xml_element, current_node)
                return subcls(xml_element)
        raise CompilerException("Invalid subaction.")
    
    def validate(self):
        pass
    
##################################
"""
    Is a possible subaction; generates an event.
"""

class RaiseEvent(SubAction):
    tag = "raise"
    UNKNOWN_SCOPE = 0
    LOCAL_SCOPE = 1
    BROAD_SCOPE = 2
    OUTPUT_SCOPE = 3
    NARROW_SCOPE = 4
    
    
    def __init__(self, xml_element, current_node):
        self.event = xml_element.get("event","").strip()
        scope_string = xml_element.get("scope","").strip().lower()
        if scope_string == "local" :
            self.scope = self.LOCAL_SCOPE
        elif scope_string == "broad" :
            self.scope = self.BROAD_SCOPE
        elif scope_string == "output" :
            self.scope = self.OUTPUT_SCOPE
        elif scope_string == "narrow" :
            self.scope = self.NARROW_SCOPE
        elif scope_string == "" :
            self.scope = self.UNKNOWN_SCOPE
        else :
            raise CompilerException("Illegal scope attribute; needs to be one of the following : local, broad, narrow, output or nothing.");

        self.target = xml_element.get("target","").strip()
        self.port = xml_element.get("port","").strip()
        if self.scope == self.UNKNOWN_SCOPE :
            if self.target and self.port :
                raise CompilerException("Both target and port attribute detected without a scope defined.")
            elif self.port :
                self.scope = self.OUTPUT_SCOPE
            elif self.target :
                self.scope = self.NARROW_SCOPE
            else :
                self.scope = self.LOCAL_SCOPE    
                
        if self.scope == self.LOCAL_SCOPE or self.scope == self.BROAD_SCOPE :
            if self.target :
                showWarning("Raise event target detected, not matching with scope. Ignored.")
                self.target = ""
            if self.port :
                showWarning("Raise event port detected, not matching with scope. Ignored.")
                self.port = ""
        if self.scope == self.NARROW_SCOPE and self.port :
            showWarning("Raise event port detected, not matching with scope. Ignored.")
            self.port = ""
        if self.scope == self.OUTPUT_SCOPE and self.target :
            showWarning("Raise event target detected, not matching with scope. Ignored.")
            self.target = ""
                
        self.params = []
        parameters = xml_element.findall('parameter')    
        for p in parameters :
            value = p.get("expr","")
            if not value :
                raise CompilerException("Parameter without value detected.")
            self.params.append(Expression(value, current_node))
   
    def __str__(self):
        presentation = "Scope  : %s \n" % self.scope
        if not self.isLocal() : 
            presentation += "Target : %s \n" % self.getNode().getFullName()
        presentation += "Event  : %s \n" % self.getEvent()
        return presentation
    
    @staticmethod
    def check(tag):
        return tag == RaiseEvent.tag
    
    def getPort(self):
        return self.port
            
    def isLocal(self):
        return self.scope == self.LOCAL_SCOPE
    
    def isNarrow(self):
        return self.scope == self.NARROW_SCOPE
    
    def isBroad(self):
        return self.scope == self.BROAD_SCOPE
    
    def isOutput(self):
        return self.scope == self.OUTPUT_SCOPE
    
    def getTarget(self):
        return self.target
    
    def getEventName(self):
        return self.event
    
    def getParameters(self):    
        return self.params
    
    def getScope(self):
        return self.scope
            
class Script(SubAction):
    tag = "script"
    def __init__(self, xml_element):
        self.code = xml_element.text
        
    @staticmethod
    def check(tag):
        return tag == Script.tag
            
class Log(SubAction):
    tag = "log"
    def __init__(self, xml_element):
        self.message = xml_element.text.strip()
        
    @staticmethod
    def check(tag):
        return tag == Log.tag
    
class AddAssociatedObject(SubAction):
    tag = "add"
    def __init__(self, xml_element):
        self.name = xml_element.get("name","")
        if self.name == "" :
            raise CompilerException("Associated object should have a unique name.");
        self.reference = xml_element.get("reference","")
        if self.reference == "" :
            raise CompilerException("A reference should be provided.");
        
    @staticmethod
    def check(tag):
        return tag == AddAssociatedObject.tag
    
class Assign(SubAction):
    tag = "assign"
    def __init__(self, xml_element, current_node):
        self.lvalue = LValue(xml_element.get("location",""))
        self.expression = Expression(xml_element.get("expr",""), current_node)
    
    @staticmethod   
    def check(tag):
        return tag == Assign.tag
    
    def validate(self):
        self.expression.validate()
  
##################################

"""
    Exists out of multiple subactions
"""
class Action(Visitable):
    def __init__(self, xml_element, current_node):
        self.sub_actions = []
        for subaction in list(xml_element) :
            if subaction.tag not in ["parameter"] :      
                self.sub_actions.append(SubAction.create(subaction, current_node))
            
    def accept(self, visitor):
        for subaction in self.sub_actions :
            subaction.accept(visitor)
            
    def validate(self):
        for subaction in self.sub_actions :
            subaction.validate()
        
##################################

class StateChartTransition():
    def __init__(self,xml_element,parent):
        self.xml = xml_element
        self.parent_node = parent
        self.parent_statechart = self.parent_node.getParentStateChart()
        self.trigger = TriggerEvent(self.xml)
        guard_string = self.xml.get("cond","").strip()
        if guard_string != "" : 
            self.guard = Expression(guard_string, self.parent_node)
        else :
            self.guard = None
        target_string = self.xml.get("target","").strip()
        if target_string == "" :
            raise CompilerException("Transition from <" + self.parent_node.getFullID() + "> has empty target.")
        self.target = Target(target_string, self.parent_node)
        
        self.action = Action(self.xml, self.parent_node)
     
    def validate(self):
        try :
            self.target.validate()
        except CompilerException as exception :
            raise CompilerException("Transition from <" + self.parent_node.getFullID() + "> has invalid target. " + exception.message)
        try :
            self.action.validate()
        except CompilerException as exception :
            raise CompilerException("Transition from <" + self.parent_node.getFullID() + "> has invalid action. " + exception.message)
        try :
            if self.guard : 
                self.guard.validate()
        except CompilerException as exception :
            raise CompilerException("Transition from <" + self.parent_node.getFullID() + "> has invalid guard. " + exception.message)
        
        
    def isUCTransition(self):
        """ Returns true iff is an unconditional transition (i.e. no trigger)
        """
        return self.trigger.isUC()
        
    def getTrigger(self):
        return self.trigger
        
    def getGuard(self):
        return self.guard
        
    def getTarget(self):
        return self.target
    
    def isAfter(self):
        return self.trigger.is_after

    def hasGuard(self):
        return self.guard != None
    
    def getAction(self):
        return self.action        

##################################  

class EnterExitAction(Visitable):
    def __init__(self, parent_node, xml_element = None):
        self.parent_node = parent_node
        if xml_element is not None:
            self.action = Action(xml_element, parent_node)
        else :
            self.action = None
            
    def validate(self):
        if self.action :
            self.action.validate()
        
class EnterAction(EnterExitAction):
    def __init__(self, parent_node, xml_element = None):
        EnterExitAction.__init__(self, parent_node, xml_element)
        
class ExitAction(EnterExitAction):
    def __init__(self, parent_node, xml_element = None):
        EnterExitAction.__init__(self, parent_node, xml_element)
        
##################################  

class StateChartNode:
    def __init__(self,xml_element,state_type, is_orthogonal, parent):
        self.children = []
        self.defaults = []        
        self.xml = xml_element
        self.is_basic = False
        self.is_composite = False
        self.is_history = False
        self.is_history_deep = False;
        self.is_root = False
        if state_type == "Basic" :
            self.is_basic = True
        elif state_type == "Composite" :
            self.is_composite = True
        elif state_type == "History" :
            history_type = xml_element.get("type","")
            if history_type == "deep" :
                self.is_history_deep = True
            elif history_type == "shallow" :
                pass
            else :
                showWarning("Invalid history type.") 
            self.is_history = True
        elif state_type == "Root" :
            self.is_root = True
                
        self.is_orthogonal = is_orthogonal
        self.parent_node = parent
        self.initial = xml_element.get("initial","")
        if self.is_root :
            self.name = "Root"
            self.full_id = "Root"
            self.parent_statechart = parent
            self.parent_node = None
            self.is_composite = True
        else :
            self.name = xml_element.get("id","")
            self.full_id = parent.getFullID() + "/" + self.name
            self.parent_statechart = parent.getParentStateChart()
            
        self.is_parallel = False
        if xml_element.tag == "parallel" :
            self.is_parallel = True
        conflict = xml_element.get("conflict","")
        if conflict == "outer" :
            self.solves_conflict_outer = True
        elif conflict == "inner" :
            self.solves_conflict_outer = False
        else :    
            if not (conflict == "" or conflict == "inherit") :
                showWarning("Unknown conflict attribute for " + self.getFullID() + ", defaulting to 'inherit'.")
            #Do our default inherit action
            if self.is_root or self.parent_node.solvesConflictsOuter(): 
                self.solves_conflict_outer = True
            else :
                self.solves_conflict_outer = False
            
        #onenter
        on_entries = xml_element.findall("onentry")
        if on_entries :
            if len(on_entries) > 1:
                showWarning("A node can only have one onentry tag! Only compiling first tag of node" + self.getFullID() + ".")
            self.entry_action = EnterAction(self, on_entries[0])
        else :
            self.entry_action = EnterAction(self)
        #onexit
        on_exits = xml_element.findall("onexit")
        if on_exits :
            if len(on_exits) > 1:
                showWarning("A node can only have one onexit tag! Only compiling first tag of node" + self.getFullID() + ".")
            self.exit_action = ExitAction(self, on_exits[0])    
        else :
            self.exit_action = ExitAction(self)
        
        #transitions
        self.transitions = []
        for transition_xml in xml_element.findall("transition"):
            transition = StateChartTransition(transition_xml,self)
            self.transitions.append(transition)
            
        #optimizing transitions
        #If a transition with no trigger and no guard is found then it is considered as the only transition.
        #Otherwise the list is ordered by placing transitions having guards only first.
        onlyguards = []
        withtriggers = []
        optimized = []
        for transition in self.transitions:
            if transition.isUCTransition():
                if not transition.hasGuard():
                    if optimized :
                        raise TransitionException("More than one transition found at a single node, that has no trigger and no guard.")
                    optimized.append(transition)
                else:
                    onlyguards.append(transition)
            else:
                withtriggers.append(transition)
        if not optimized :        
            optimized = onlyguards + withtriggers
        self.transitions = optimized
            
        #Some checks
        assert (not self.isOrthogonal()) or (self.isBasic() or self.isComposite())
        
    def getFullID(self):
        return self.full_id
        
    def getFullName(self):
        return self.full_id.replace("/","_")
             
    def isOrthogonal(self):
        return self.is_orthogonal
    
    #Means that the children are orthogonal
    def isParallel(self):
        return self.is_parallel
    
    def isBasic(self):
        return self.is_basic

    def isComposite(self):
        return self.is_composite
    
    def isHistory(self):
        return self.is_history
    
    def getName(self):
        return self.name
        
    def getXML(self):
        return self.xml
    
    def getParentNode(self):
        return self.parent_node
    
    def getParentStateChart(self):
        return self.parent_statechart
    
    def getChildren(self):
        return self.children
    
    def getDefaults(self):
        return self.defaults
        
    def getTransitions(self):
        return self.transitions
        
    def isHistoryDeep(self):
        return self.is_history_deep
        
    def getEnterAction(self):
        return self.entry_action
        
    def getExitAction(self):
        return self.exit_action
    
    def getInitial(self):
        return self.initial
    
    def solvesConflictsOuter(self):
        return self.solves_conflict_outer
    
    
    def getAfterTransitions(self):
        """ Returns a list of transitions out of the node which are triggred by AFTER()
        """
        return [transition for transition in self.transitions if transition.isAfter()]
    
    def validate(self):
        self.entry_action.validate()
        self.exit_action.validate()
        for transition in self.transitions :
            transition.validate()
        
##################################

class StateChart(Visitable):

    def __init__(self, statechart_xml, className):
        """ Gives the module information on the statechart by listing its basic, orthogonal,
            composite and history states as well as mapping triggers to names making the
            appropriate conversion from AFTER() triggers to event names
        """
        
        self.root = StateChartNode(statechart_xml,"Root", False, self);
        self.className = className
        self.initTimers = []
        self.succeeded = True

        self.basics = []
        self.composites = []
        self.historys = []

        self.number_time_transitions = 0
        self.addToHierarchy(self.root)  
            
        # Calculate the history that needs to be taken care of.
        self.historyParents = []
        for node in self.historys:
            self.calculateHistory(node.getParentNode(), node.isHistoryDeep())

        self.calculateAfters()
        
        #do semantic additions and validation
        for node in self.basics + self.composites:
            node.validate()
        
    def addToHierarchy(self,parent) :
        children_names = []
        children = []
        for xml_node in list(parent.xml) :
            state_type = "Basic"
            is_orthogonal = False    
                
            if xml_node.tag == "parallel" : 
                state_type = "Composite" 
            elif xml_node.tag == "state" :
                if len(xml_node.findall("state")) > 0 or (len(xml_node.findall("parallel")) > 0) :
                    state_type = "Composite"
                if  parent.xml.tag == "parallel" :
                    is_orthogonal = True
            elif xml_node.tag == "history" :
                state_type = "History"
            else :
                continue
             
            node = StateChartNode(xml_node, state_type, is_orthogonal, parent)     
            child_name = node.getName()
            if child_name == "" :
                raise CompilerException("Found state with no id")
            if child_name in children_names :
                raise CompilerException("Found 2 equivalent id's : " + child_name + ".")
            children_names.append(child_name)                
                    
            append_child = True
            if node.isBasic() : 
                self.basics.append(node)
            elif node.isComposite() : 
                self.composites.append(node)
            elif node.isHistory() : 
                self.historys.append(node)
            else :
                append_child = False
            if append_child :
                children.append(node)
            parent.children = children

        #calculating and validating defaultness
        if parent.isParallel() :
            parent.defaults = [x for x in children if not x.isHistory()]
            if parent.getInitial() != "" : 
                showWarning("Component <" + parent.getFullID() + "> in class <" + self.className + ">. contains an initial state while being parallel. Ignoring.")    
        elif parent.getInitial() == "" :
            if parent.isBasic() or parent.isHistory():
                pass
            elif len(children) == 1 :
                parent.defaults = children
            else :
                raise CompilerException("Component <" + parent.getFullID() + "> contains no default state.") 
        else :
            if parent.isBasic() :
                raise CompilerException("Component <" + parent.getFullID() + "> contains a default state while being a basic state.")
            initialChilds = []
            for s in children :
                if s.getName() == parent.getInitial() :
                    initialChilds.append(s)
            if len(initialChilds) < 1 :
                raise CompilerException("Initial state '"+ parent.getInitial() + "' referred to, is missing in " + parent.getFullID())
            elif len(initialChilds) > 1 :
                raise CompilerException("Multiple states with the name '" + parent.getInitial() + " found in " + parent.getFullID() + " which is referred to as initial state.")
            parent.defaults = initialChilds       
            
        # For each AFTER event, give it a name so that it can be triggered.
        for transition in parent.getTransitions():
            if transition.isAfter() :
                trigger = transition.getTrigger()
                trigger.setAfterIndex(self.number_time_transitions)
                value = "_" + str(trigger.getAfterIndex()) + "after"
                trigger.setEvent(value)
                self.number_time_transitions += 1
            
        for child in children :
            self.addToHierarchy(child)

    def calculateHistory(self, parent, is_deep):
        """ Figures out which components need to be kept track of for history.
        """
        if parent == self.root:
            showWarning("Root component cannot contain history in class <" + self.className + ">. Not processed!")
        if parent not in self.historyParents:
            self.historyParents.append(parent)
        if parent.isParallel() or is_deep :
            for i in parent.children:
                if i.isComposite() :
                    self.calculateHistory(i, is_deep)

    def calculateAfters(self):
        self.afterNodeEvents = {}
        for node in self.composites + self.basics:
            if node in self.afterNodeEvents:
                timers = self.afterNodeEvents[node]
            else:
                timers= []
            for ae in node.getAfterTransitions():
                timers.append((ae.getTrigger().getAfterIndex(), ae.getTrigger().getAfterExp()))
            self.afterNodeEvents[node] = timers
        
    def getOuterNodes(self, node):
        """ Returns a list representing the containment hierarchy of node.
            node is always the first element, and its outermost parent is the last.
        """
        outernodes=[]
        while node != self.root :
            outernodes.append(node)
            node = node.getParentNode()
        outernodes.append(self.root)
        return outernodes

    def getTransitionPath(self, startnode, endnode):
        """ Computes the states that must be exited and entered if the system is to make
            a transition from 'startnode' to 'endnode'. These ordered lists are returned
            in a tuple, (exitedNodes, enteredNodes).
        """
        #first get the ancestors of each node
        startAncestors=self.getOuterNodes(startnode)
        endAncestors=self.getOuterNodes(endnode)

        #now find the scope of the transition (lowest common proper ancestor)
        LCA=None
        while not(startAncestors[-1] == startnode or
                  endAncestors[-1] == endnode or
                  startAncestors[-1] != endAncestors[-1]):
            LCA=startAncestors.pop()
            endAncestors.pop()

        #the states to be exited are all proper descendants of the scope in which
        #currentState resides
        #the states to be entered are all the proper descendants of the scope in
        #which the final basic state resides
        enterpath = self.getOuterNodes(endnode)
        exitpath = self.getOuterNodes(startnode)
        if LCA:
            exitpath=exitpath[:exitpath.index(LCA)]    ## last element (LCA) is
            enterpath=enterpath[:enterpath.index(LCA)] ## not included in slice
        enterpath.reverse()

        return (exitpath, enterpath)
    
    def accept(self, visitor):
        visitor.enter(self)
        for i in self.composites + self.basics:
            i.getEnterAction().accept(visitor)
            i.getExitAction().accept(visitor)
        visitor.exit(self)

###################################

class Association(Visitable):
    def __init__(self, from_class, to_class, min_card, max_card):
        self.min = min_card
        self.max = max_card
        self.from_class = from_class
        self.to_class = to_class
        
###################################
class FormalParameter(Visitable):
    def __init__(self, param_ident, param_type, default = None):
        self.param_type = param_type
        self.identifier = param_ident
        self.default = default
        if self.default == "" :
            self.default = None    
            
    def getType(self):
        return self.type
            
    def getIdent(self):
        return self.identifier
    
    def hasDefault(self):
        return self.default is not None
    
    def getDefault(self):
        return self.default
    
###################################
class Method(Visitable):
    def __init__(self, xml, parent_class):
        self.name = xml.get("name", "");
        parameters = xml.findall("parameter")
        self.parameters = []
        for p in parameters:
            self.parameters.append(FormalParameter(p.get("name",""), p.get("type", "")), p.get("default",""))
        self.body = xml.text
        self.parent_class = parent_class
        self.return_type = xml.get('type',"")
        
    def getParams(self):
        return self.parameters
        
###################################        
class Constructor(Method):
    def __init__(self, xml, parent_class):
        if xml is None :
            self.body = ""
            self.name = ""
            self.parent_class = parent_class
            self.return_type = ""
            self.parameters = []
        else :
            Method.__init__(self, xml, parent_class)      
        
class Destructor(Method):
    def __init__(self, xml, parent_class):
        Method.__init__(self, xml, parent_class)  
        
###################################         
class Attribute(Visitable):
    def __init__(self, xml):
        self.name = xml.get('name',"")
        self.type = xml.get('type',"")
        self.init_value = xml.get("init-value","")
        
###################################

class Class(Visitable):
    def __init__(self, xml):

        self.xml = xml
        self.name = xml.get("name", "")
        self.constructors = []
        self.destructors = []
        self.methods = []
        self.statechart = None
        
        self.attributes = []
        self.associations = []
        self.super_classes = []
        
        self.process()
        
    def getClassName(self):
        return self.name
    
    def getAssociations(self):
        return self.associations
    
    def accept(self, visitor):
        visitor.enter(self)
        for i in self.constructors :
            i.accept(visitor)
        for i in self.destructors :
            i.accept(visitor)
        for i in self.methods :
            i.accept(visitor)
        if self.statechart is not None:
            self.statechart.accept(visitor)
        visitor.exit(self)
        
    def processMethod(self, method_xml) :
        name = method_xml.get("name", "")
        if name == self.name :
            self.constructors.append(Constructor(method_xml, self))
        elif name == '~' + self.name:
            self.destructors.append(Destructor(method_xml, self))
        else :
            if name in reserved:
                raise CompilerException("Reserved word \"" + name + "\" used as method in class <" + self.name + ">.")
            self.methods.append( Method(method_xml, self))

    def processAttribute(self, attribute_xml):
        attribute = Attribute(attribute_xml)
        if attribute.name in reserved:
            raise CompilerException("Reserved word \"" + attribute.name + "\" used as variable in class <" + self.name + ">.")

        self.attributes.append(attribute)
    
    def processInheritances(self, inheritances):
        # process each inheritance, stores a dict with each subclass as the key
        # and a list of tuples (superclass, priority) as the value. The priority
        # tells us which class to inherit from first for multiple inheritance. Gives
        # a WARNING with a given inheritance order if two priorities are the same
        for i in inheritances :
            self.super_classes.append((i.get("class",""),i.get("priority",1)))
            
        self.super_classes.sort(lambda a, b: cmp(a[1], b[1]))
        priorityChecker = {}
        for super_class, priority in self.super_classes:
            if priority in priorityChecker:
                checkIt = priorityChecker[priority]
            else:
                checkIt = []
            if super_class not in checkIt:
                checkIt.append(super_class)
            priorityChecker[priority] = checkIt
        for priority, checkIt in priorityChecker.iteritems():
            if len(checkIt) > 1:
                showWarning("Class <" + self.name + "> inherits from classes <" + ", ".join(checkIt) + "> with same priority <" + str(priority) + ">. Given inheritance order is chosen.")
                
        self.super_classes = [entry[0] for entry in self.super_classes]        

    def process(self):
        attributes = self.xml.findall("attribute")
        for a in attributes:
            self.processAttribute(a)

        methods = self.xml.findall("method")
        for m in methods:
            self.processMethod(m)
            
        if len(self.constructors) < 1 :
            #add a default constructor
            self.constructors.append(Constructor(None,self))

        associations = []
        inheritances = []
        relationships = self.xml.findall("relationships")
        for relationship_wrapper in relationships :
            associations.extend(relationship_wrapper.findall("association"))
            inheritances.extend(relationship_wrapper.findall("inheritance"))
            
        for a in associations :
            self.associations.append(
                Association(self.name,
                            a.get("class",""),
                            a.get("card-min","0"),
                            a.get("card-max","N"))
            )
        self.processInheritances(inheritances)

        statecharts = self.xml.findall("scxml")
        if len(statecharts) > 1 :
            raise CompilerException("Multiple statecharts found in class <" + self.name + ">.")
        if len(statecharts) == 1 :
            self.statechart = StateChart(statecharts[0], self.name)
            
###################################
class ClassDiagram(Visitable):
    def __init__(self, input_file):
        self.source = input_file
        tree = ET.parse(self.source)
        self.root = tree.getroot()
        self.name = self.root.get("name", "")
        self.author = self.root.get("author", "")
        descriptions = self.root.findall("description")
        if descriptions : 
            self.description = descriptions[0].text
        else :
            self.description = "No description provided"
    
        xml_classes = self.root.findall("class")
        # make sure at least one class is given
        if not xml_classes :
            raise CompilerException("Found no classes to compile.")
    
        # check if class diagram is valid
        # unique class names
        self.class_names = []
        for xml_class in xml_classes :
            name = xml_class.get("name", "")
            if name == "" :
                raise CompilerException("Missing or emtpy class name.")
            if name in self.class_names :
                raise CompilerException("Found 2 classes with the same name : " + name + ".")
            self.class_names.append(name)
    
        # if only one class is given, then treat it as the default regardless
        default_class_xml = None
        self.default_class = None
        if len(xml_classes) == 1:
            default_class_xml = xml_classes[0]
            showInfo("Only one class given. Using <" + default_class_xml.get("name", "") + "> as the default class.")
        # otherwise make sure only one default class given
        else:
            has_default = False
            for xml_class in xml_classes:
                if xml_class.get("default", "") == "true" :
                    if has_default:
                        raise CompilerException("Found two default classes: <" + default_class_xml.get("name", "") + "> and <" + xml_class.get("name", "") + ">.")
                    else:
                        has_default = True
                        default_class_xml = xml_class
            if not has_default:
                raise CompilerException("No default class provided.")
    
        # process in and output ports
        inports = self.root.findall("inport")
        names = []
        for xml_inport in inports :
            name = xml_inport.get("name", "")
            if name in names :
                raise CompilerException("Found 2 INPorts with the same name : " + name + ".")
            names.append(name)
        self.inports = names
        
        outports = self.root.findall("outport")
        names = []
        for xml_outport in outports :
            name = xml_outport.get("name", "")
            if name in names :
                raise CompilerException("Found 2 OUTPorts with the same name : " + name + ".")
            names.append(name)
        self.outports = names
        
        self.protocol = xml_class.get("protocol", "threads").lower()
        if self.protocol not in ["threads", "gameloop"] :
            raise CompilerException("Invalid protocol : " + self.protocol + ".")
            
        
        # imports/includes
        """includes = self.root.findall("include")
        self.includes = []
        for xml_include in includes :
            path = xml_include.get("path", "")
            if path :
                self.includes.append()
        """
        
        # process each class in diagram
        self.classes = []
    
        for xml_class in xml_classes:
            processed_class = None
            try :
                processed_class = Class(xml_class)
            except CompilerException as e :
                e.message = "Class <" + xml_class.get("name", "") + "> failed compilation. " + e.message
                raise e
    
            # Change defaultClass to it's processed version
            if default_class_xml == xml_class:
                self.default_class = processed_class
    
            # let user know this class was successfully loaded
            showInfo("Class <" + processed_class.name + "> has been successfully loaded.")
            self.classes.append(processed_class)
            
    def accept(self, visitor):
        visitor.enter(self)
        for c in self.classes :
            c.accept(visitor)
        visitor.exit(self)

###################################
class CompilerException(Exception):
    def __init__(self, message):
        self.message = message
    def __str__(self):
        return repr(self.message)
    
class TransitionException(CompilerException):
    pass
    
###################################

verbose = 0 #0 = no output
            #1 = only warnings
            #2 = all output

def showWarning(warning):
    if(verbose > 0) :
        print "WARNING : " + warning
        
def showInfo(info):
    if(verbose > 1) :
        print "INFO : " + info
        
###################################
   
def generate(input_file, output_file, target_code = "Python"):
    class_diagram = process(input_file)
    _generate(class_diagram, output_file, target_code)
      
def process(input_file):
    return ClassDiagram(input_file)
    
def _generate(class_diagram, output_file, target_code = "Python"):
    target_code = target_code.lower()
    if target_code == "python" :
        Python.PythonGenerator(class_diagram, output_file).generate()
    elif target_code == "csharp" or target_code == "c#" :
        showWarning("C# generation not implemented yet.")
    # let user know ALL classes have been processed and loaded
    showInfo("The following classes <" + ", ".join(class_diagram.class_names) + "> have been exported to the following file: " + output_file)
        
###################################



def main():
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument('source', help='The path to the XML file to be compiled.')
    parser.add_argument('-t', '--target', type=str, help='The path to the target python file. Defaults to the same name as the source file.')
    parser.add_argument('-v', '--verbose', type=int, help='0 = no output, 1 = only show warnings, 2 = show all output. Defaults to 1.', default = 1)
    
    args = vars(parser.parse_args())

    source = args['source'].lower()
    if not source.endswith(".xml") :
        print "Input file not valid"
        return
    if args['target'] :
        target = args['target'].lower()
        if not target.endswith(".py") :
            print "Output file not valid"
            return
    else :
        target = os.path.splitext(os.path.split(source)[1])[0] + ".py"
    global verbose
    if args['verbose'] :
        if args['verbose'] in [0,1,2] :
            verbose = args['verbose']
        else :
            print "Invalid verbose argument"
    else :
        verbose = 2
        
    try :
        generate(source, target)
    except CompilerException as exception :
        print exception

if __name__ == "__main__":
    main()


