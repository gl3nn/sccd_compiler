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
PORT_CHAR = ':'
BROADCAST_CHAR = '*'
NARROWCAST_CHAR = '}'

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
            if parent in self.statechart.hierarchy : 
                for child in self.statechart.hierarchy[parent] :
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
    def __init__(self, string):
        self.string = string
        #some validation should be done here as well.. now it doesn't do much besides wrapping a string
        
    def getString(self):
        return self.string
    
##################################
class TriggerEvent:
    def __init__(self, eventString):
        self.port = ""
        self.scope = "local"
        self.is_uc = False;
        self.is_after = False
        self.after_index = -1
        self.after_exp = ""
        self.params = []
        self.event = eventString.replace(' ', '') # remove spaces
        
        if eventString == "" :
            self.is_uc = True
            return
        
        if len(self.event) > 7 and self.event[0:6] == "AFTER(" and self.event[-1] == ")":
            self.is_after = True
            self.after_exp = self.event[6:-1]
            return
  
        beginIndex = self.event.find(PORT_CHAR) # check for port
        if beginIndex > 0 :
            self.scope      = "port"
            self.port       = self.event[0:beginIndex]
            self.event      = self.event[beginIndex+1:]
        elif beginIndex == 0 :
            raise CompilerException("Event has the port character ':' at the wrong position.")
     
        
        params_index = self.event.find('(')
        if params_index > 0 :
            for a in self.event[params_index+1:-1].split(","):
                self.params.append(FormalEventParameter(a))       
            self.event = self.event[:params_index]
        elif params_index == 0 :
            raise CompilerException("Event has the parameter character '(' at the wrong position.")
    
    def getEvent(self):    
        return self.event
    
    def setEvent(self, event):
        self.event = event
    
    def getParameters(self):    
        return self.params
    
    def getPort(self):
        return self.port
    
    def isLocal(self):
        return self.scope == "local"
    
    def hasPort(self):
        return self.scope == "port"
    
    def isUC(self):
        return self.is_uc;
    
    def isAfter(self):
        return self.is_after

    def getAfterExp(self):
        return self.after_exp
    
    def getAfterIndex(self):
        return self.after_index
    
    def setAfterIndex(self, after):
        self.after_index = str(after)
        
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
    
    def __init__(self, xml_element, current_node):
        event_string = xml_element.get("event","").strip()
        self.target = ""
        self.port = ""
        event_string = event_string.replace(' ', '') # remove spaces
        port_index = event_string.find(PORT_CHAR)
        has_port = port_index > -1
        broadcast_index = event_string.find(BROADCAST_CHAR)
        has_broadcast = broadcast_index > -1
        narrow_index = event_string.find(NARROWCAST_CHAR) 
        has_narrow = narrow_index > -1
        self.params = []
        if sum([has_port, has_broadcast, has_narrow]) > 1 :
            raise CompilerException("Event can be just one of the following : local, broadcast(*), narrowcast(.) or output(:).");
        #Check for broadcast
        if has_broadcast :
            if broadcast_index == 0 :
                self.scope      = "broad"
                self.event      = event_string[broadcast_index+1:]
            else :                       
                raise CompilerException("Event has the broadcast character '*' at the wrong position.");
        #Check for narrowcast
        elif has_narrow :
            if narrow_index > 0 :
                self.scope      = "narrow"
                self.event      = event_string[narrow_index+1:]
                self.target     = event_string[0:narrow_index]
            else :
                raise CompilerException("Event has the narrowcast character '.' at the wrong position.");
        #Check for output
        elif has_port:
            if port_index > 0 :
                self.scope      = "output"
                self.event      = event_string[port_index+1:]
                self.port       = event_string[0:port_index]
            else:
                raise CompilerException("Event has the output character ':' at the wrong position.");
        else :
            #should be local
            self.scope      = "local"
            self.event      = event_string
            
        params_index = self.event.find('(')
        if params_index > 0 :
            for a in self.event[params_index+1:-1].split(","):
                self.params.append(Expression(a, current_node))       
            self.event = self.event[:params_index]
        elif params_index == 0 :
            raise CompilerException("Event has the parameter character '(' at the wrong position.");
   
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
        return self.scope == "local"
    
    def isNarrow(self):
        return self.scope == "narrow"
    
    def isBroad(self):
        return self.scope == "broad"
    
    def isOutput(self):
        return self.scope == "output"
    
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
        self.trigger = TriggerEvent(self.xml.get("event","").strip())
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
            history_type = self.xml.get("type","")
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
        self.initial = self.xml.get("initial","")
        if self.is_root :
            self.name = "Root"
            self.full_id = "Root"
            self.parent_statechart = parent
            self.parent_node = None
            self.is_composite = True
        else :
            self.name = self.xml.get("id","")
            self.full_id = parent.getFullID() + "/" + self.name
            self.parent_statechart = parent.getParentStateChart()
        on_entries = self.xml.findall("onentry")
        if on_entries :
            if len(on_entries) > 1:
                showWarning("A node can only have one onentry tag! Only compiling first tag of node" + self.getFullID() + ".")
            self.entry_action = EnterAction(self, on_entries[0])
        else :
            self.entry_action = EnterAction(self)
        on_exits = self.xml.findall("onexit")
        if on_exits :
            if len(on_exits) > 1:
                showWarning("A node can only have one onexit tag! Only compiling first tag of node" + self.getFullID() + ".")
            self.exit_action = ExitAction(self, on_exits[0])    
        else :
            self.exit_action = ExitAction(self)

        self.transitions = []
        for transition_xml in self.xml.findall("transition"):
            transition = StateChartTransition(transition_xml,self)
            self.transitions.append(transition)
        
        self.is_parallel = False
        if self.xml.tag == "parallel" :
            self.is_parallel = True
    
        conflict = self.xml.get("conflict","")
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
    
    def getTransitionsOutOf(self):
        """ Returns an optimized list of all hyperedges coming out of 'node'.
            If a hyperedge with no trigger or guard is found then it alone is returned.
            Otherwise the returned list is ordered by hyperedges having guards only,
            followed by the rest of the hyperedges. This is done to facilitate the
            sequence of transitions executed in a single step (loop iteration).
        """
        onlyguards = []
        withtriggers = []
        for transition in self.getTransitions():
            if transition.isUCTransition():
                if not transition.hasGuard():
                    return [transition]
                else:
                    onlyguards.append(transition)
            else:
                withtriggers.append(transition)
        return onlyguards + withtriggers
    
    def getAfterTransitions(self):
        """ Returns a list of edges out of the node which are triggred by AFTER()
        """
        afters = []
        for transition in self.getTransitionsOutOf():
            if transition.isAfter():
                afters.append(transition)
        return afters
    
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

        self.hierarchy = {} #Maps parents to childs
        self.defaults = {}
        self.inits = []

        self.basics = []
        self.composites = []
        self.historys = []
    
        self.addToHierarchy(self.root)
        
        # Verify that a component is not empty and doesn't contain two nodes with the same name!

        for k, v in self.hierarchy.iteritems() :
            if v == []:
                raise CompilerException("Empty component <" + k.getFullID() + "> in class <" + self.className + ">.")        
            state_ids = []
            for child in v :
                state_id = child.getName()
                if state_id == "" :
                    raise CompilerException("Found state with no id")
                else :
                    if state_id in state_ids :
                        raise CompilerException("Found 2 ids the same : " + state_id)
                    state_ids.append(state_id)                    

        # Check default nodes and complete the initial path of the stateChart.
        self.checkDefaults()
        for i in self.defaults[self.root] :
            self.completeInits(i)

        # For each AFTER event, give it a name so that it can be triggered.
        i_after = 0
        for k, v in self.hierarchy.iteritems() :
            for element in v :
                for transition in element.getTransitions():
                    if transition.isAfter() :
                        trigger = transition.getTrigger()
                        trigger.setAfterIndex(i_after)
                        value = "_" + trigger.getAfterIndex() + "after"
                        trigger.setEvent(value)
                        i_after += 1
                    
        self.numberTimeTransitions = i_after
                        

        # Calculate the possible transitions of the statechart.
        self.transitions = {}
        for node in self.basics + self.composites:
            self.transitions[node] = node.getTransitionsOutOf()
            
        # Calculate the history that needs to be taken care of.
        self.historyParents = []
        for node in self.historys:
            self.calculateHistory(node.getParentNode(), node.isHistoryDeep())

        self.calculateAfters()
        
        #do semantic additions and validation
        for node in self.basics + self.composites:
            node.validate()
        
    def addToHierarchy(self,parent) :
        children = []
        for element in list(parent.xml) :
            state_type = "Basic"
            is_orthogonal = False    
                
            if element.tag == "parallel" : 
                state_type = "Composite"
                 
            elif element.tag == "state" :
                if len(element.findall("state")) > 0 or (len(element.findall("parallel")) > 0) :
                    state_type = "Composite"
                    
                if  parent.xml.tag == "parallel" :
                    is_orthogonal = True
                    
            elif element.tag == "history" :
                state_type = "History"
                
            else :
                continue
             
            node = StateChartNode(element, state_type, is_orthogonal, parent)
            if node.isBasic() : 
                self.basics.append(node)
                children.append(node)
            elif node.isComposite() : 
                self.composites.append(node)
                children.append(node)
            elif node.isHistory() : 
                self.historys.append(node)
                children.append(node)
            
        if children :
            self.hierarchy[parent] = children
            
        for child in children :
            self.addToHierarchy(child)
                

    def checkDefaults(self):
        """ Verifies if all semantics and default-ness are met i.e. every component has a default
            state, orthogonals are solely in a given component, etc...
        """

        for k, v in self.hierarchy.iteritems():
            if k.isParallel() :
                self.defaults[k] = [x for x in v if not x.isHistory()]
                if k.getInitial() != "" : 
                    showWarning("Component <" + k.getFullID() + "> in class <" + self.className + ">. contains an initial state while being parallel. Ignoring.")    
            elif k.getInitial() == "" :
                if len(v) == 1 :
                    self.defaults[k] = v
                else :
                    raise CompilerException("Component <" + k.getFullID() + "> contains no default state.") 
            else :
                initialChilds = []
                for s in v :
                    if s.getName() == k.getInitial() :
                        initialChilds.append(s)
                if len(initialChilds) < 1 :
                    raise CompilerException("Initial state '"+ k.getInitial() + "' referred to, is missing in " + k.getFullID())
                elif len(initialChilds) > 1 :
                    raise CompilerException("Multiple states with the name '" + k.getInitial() + " found in " + k.getFullID() + " which is referred to as initial state.")
                self.defaults[k] = initialChilds        

    def completeInits(self, node):
        """Calculates the complete default, initial path
           from top-level default node to basic state(s)
        """
        if node.isBasic()  or node.isHistory()  :
            if node not in self.inits :
                self.inits.append(node)              
        elif node.isComposite() :
            if node not in self.inits:
                self.inits.append(node)
            for i in self.defaults[node]:
                self.completeInits(i) 
        else :
            raise CompilerException("The type/class of a state/node wasn't set. This probably points out a bug in the compiler.")

    def calculateHistory(self, parent, is_deep):
        """ Figures out which components need to be kept track of for history.
        """
        if parent == self.root:
            showWarning("Root component cannot contain history in class <" + self.className + ">. Not processed!")
        if parent not in self.historyParents:
            self.historyParents.append(parent)
        if parent.isParallel() or is_deep :
            for i in self.hierarchy[parent]:
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
            self.parameters.append(FormalParameter(p.get("identifier",""), p.get("type", "")), p.get("default",""))
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
        if name == "__init__" :
            self.constructors.append(Constructor(method_xml, self))
        elif name == "__del__" :
            self.constructors.append(Destructor(method_xml, self))
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
            self.super_classes.append((i.get("class-relation",""),i.get("priority",1)))
            
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

        relationships = self.xml.findall("relationships")
        assert len(relationships) <= 1
        
        if len(relationships) > 0:
            associations = relationships[0].findall("association")
            for a in associations :
                self.associations.append(
                    Association(self.name,
                                a.get("class-relation",""),
                                a.get("card_min","0"),
                                a.get("card_max","N"))
                )
        
            self.processInheritances(relationships[0].findall("inheritance"))

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
        class_names = []
        for xml_class in xml_classes :
            name = xml_class.get("name", "")
            if name == "" :
                raise CompilerException("Missing or emtpy class name.")
            if name in class_names :
                raise CompilerException("Found 2 classes with the same name : " + name + ".")
            class_names.append(name)
    
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
        inports_wrappers = self.root.findall("inports")
        inports = []
        for wrapper in inports_wrappers :
            inports.extend(wrapper.findall("port"))
        names = []
        for xml_inport in inports :
            name = xml_inport.get("name", "")
            if name in names :
                raise CompilerException("Found 2 INPorts with the same name : " + name + ".")
            names.append(name)
        self.inports = names
        
        outports_wrappers = self.root.findall("outports");
        outports = []
        for wrapper in outports_wrappers :
            outports.extend(wrapper.findall("port"));
        names = []
        for xml_outport in outports :
            name = xml_outport.get("name", "")
            if name in names :
                raise CompilerException("Found 2 OUTPorts with the same name : " + name + ".")
            names.append(name)
        self.outports = names
        
        # process in and output ports
        includes_wrappers = self.root.findall("includes")
        includes = []
        for wrapper in includes_wrappers :
            includes.extend(wrapper.findall("include"))
        names = []
        for xml_include in includes :
            name = xml_include.get("path", "")
            if name in names :
                continue
            names.append(name)
        self.includes = names
        
        # process each class in diagram
        self.classes = []
    
        for xml_class in xml_classes:
            processed_class = None
            try :
                processed_class = Class(xml_class)
            except CompilerException as e :
                raise CompilerException("Class <" + xml_class.get("name", "") + "> failed compilation. " + e.message)
    
            # Change defaultClass to it's processed version
            if default_class_xml == xml_class:
                self.default_class = processed_class
    
            # let user know this class was successfully loaded
            showInfo("Class <" + processed_class.name + "> has been successfully loaded.")
            self.classes.append(processed_class)
            
        self.class_names_string = ", ".join(class_names)
            
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
    showInfo("The following classes <" + class_diagram.class_names_string + "> have been exported to the following file: " + output_file)
        
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


