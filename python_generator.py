import utils.StringUtils as StringUtils
import time
import SSC
from visitor import CodeGenerator

class PythonGenerator(CodeGenerator):
            
    def enter_ClassDiagram(self, classdiagram):
        self.fOut.write("# Statechart compiler by Glenn De Jonghe")
        self.fOut.write("#")
        self.fOut.write("# Source: " + classdiagram.source)
        self.fOut.write("# Date:   " + time.asctime())
        if classdiagram.name or classdiagram.author or classdiagram.description:
            self.fOut.write()
        if classdiagram.author:
            self.fOut.write("# Model Author: " + classdiagram.author)
        if classdiagram.name:
            self.fOut.write("# Model name:   " + classdiagram.name)
        if classdiagram.description.strip():
            self.fOut.write("# Model description:")
            self.fOut.write('"""')
            self.fOut.indent()
            self.fOut.write(classdiagram.description.strip())
            self.fOut.dedent()
            self.fOut.write('"""')
        self.fOut.write()
        
        #Mandatory imports
        self.fOut.write('import sys')
        self.fOut.write('from python_runtime.statecharts_core import ObjectManagerBase, Event, Association, AssociationInfo, ControllerBase')
        self.fOut.write()
        
    def exit_ClassDiagram(self, class_diagram):            
        # ObjectManager
        self.fOut.write('class ObjectManager (ObjectManagerBase):')
        self.fOut.indent()
        self.fOut.write('def __init__(self, controller):')
        self.fOut.indent()
        self.fOut.write("ObjectManagerBase.__init__(self, controller)")
        for c in class_diagram.classes :
            self.fOut.write('self.associations_info["' + c.getClassName() + '"] = []')
            for association in c.getAssociations() :
                association.accept(self)
        self.fOut.dedent()
        self.fOut.write()
        
        self.fOut.write('def setupObject(self, new_object):')
        self.fOut.indent()
        for c in class_diagram.classes :
            class_name = c.getClassName()
            self.fOut.write('if new_object.class_name == "' + class_name + '" :')
            self.fOut.indent()
            if c.statechart :
                self.fOut.write('new_object.reference = ' + class_name + '(self.currentTime, self.controller)')
            else :
                self.fOut.write('new_object.reference = ' + class_name + '()')
            self.fOut.write('for association_info in self.associations_info["' + class_name + '"] :')
            self.fOut.indent()
            self.fOut.write('new_object.associations[association_info.class_name] = Association(association_info)')
            self.fOut.dedent()
            self.fOut.dedent()
        
        self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.write()

        # write out controller
        self.fOut.write("# CONTROLLER BEGINS HERE")
        self.fOut.write("class Controller(ControllerBase):")
        self.fOut.indent()
    
        # write out __init__ method
        self.fOut.write("def __init__(self, keep_running = True, loopMax = 1000):")
        self.fOut.indent()
        self.fOut.write("ControllerBase.__init__(self, ObjectManager(self), keep_running, loopMax)")
        for i in class_diagram.inports:
            self.fOut.write('self.addInputPort("' + i + '")')
        for i in class_diagram.outports:
            self.fOut.write('self.addOutputPort("' + i + '")')
        self.fOut.write('self.object_manager.createDefaultInstance("'+ class_diagram.default_class.name +'")')
        self.fOut.write()
        self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.write("def main():")
        self.fOut.indent()
        self.fOut.write("controller = Controller()")
        self.fOut.write("controller.start()")
        self.fOut.dedent()
        self.fOut.write()
    
        self.fOut.write('if __name__ == "__main__":')
        self.fOut.indent()
        self.fOut.write("main()")
        self.fOut.dedent()
        self.fOut.write()

    def enter_Class(self, class_node):
        self.fOut.write()
        # take care of inheritance
        if class_node.super_classes:
            super_classes = []
            for super_class in class_node.super_classes:
                super_classes.append(super_class)
            self.fOut.write("class " + class_node.name + "(" + ", ".join(super_classes) +  "):")
        else:
            self.fOut.write("class " + class_node.name + ":")

        self.fOut.indent()
        self.fOut.write()
        
        if class_node.statechart is not None:
            # assign each node a unique ID
            self.fOut.write("# Unique IDs for all statechart nodes")
            self.fOut.write("Root = 0")
            j = 1
            for i in class_node.statechart.composites + class_node.statechart.basics:
                self.fOut.write(i.getFullName() + " = " + str(j))
                j += 1
    
            self.fOut.write()
            self.writeStateChartInitMethod(class_node)
            self.writeMethodSignature("commonConstructor", [SSC.FormalParameter("currentTime", "", "0.0"), SSC.FormalParameter("controller", "", "None"), SSC.FormalParameter("loopMax", "", "1000")])
            #self.fOut.write(" def commonConstructor(self, currentTime = 0.0, controller = None, loopMax = 1000): ")
        else :
            self.writeMethodSignature("commonConstructor",[])
        self.fOut.indent()

        # write our attributes
        if class_node.attributes:
            self.fOut.write("# User defined attributes")
            for attribute in class_node.attributes:
                if attribute.type.lower() == "string":
                    self.fOut.write("self." + attribute.name + ' = "' + attribute.init_value + '"')
                else:
                    self.fOut.write("self." +  attribute.name + " = " + attribute.init_value)
            self.fOut.write()

        # write out association variables
        self.fOut.write("# Association variables")
        self.fOut.write("self.associates = {}")
        self.fOut.write()

        # if there is a statechart
        if class_node.statechart is not None:
            self.fOut.write("self.controller = controller")
            self.fOut.write("self.associates['objectManager'] = controller.object_manager")
            
            self.fOut.write()
            self.fOut.write("# Statechart variables")
            self.fOut.write("self.currentTime = currentTime")
            self.fOut.write("self.loopMax = loopMax")
            self.fOut.write()
            self.fOut.write("# State of statechart")
            self.fOut.write("self.currentState = {}")
            self.fOut.write("self.historyState = {}")
            self.fOut.write()
            if class_node.statechart.number_time_transitions:
                self.fOut.write("# AFTER events of statechart")
                self.fOut.write("self.timers = []")
                self.fOut.write()
            self.fOut.write("# Initialize statechart")
            self.fOut.write("self.init()")
            self.fOut.write()
        self.fOut.dedent()


                
    #helper method
    def writeStateChartInitMethod(self, parent_class):
        # the following method isn't part of the actual constructor, but deals with statechart initialization, so let's leave it here :)
        self.fOut.write("def init(self):")
        self.fOut.indent()
        self.fOut.write()
        self.fOut.write("# Statechart variables")
        self.fOut.write("self.eventQueue = []")
        self.fOut.write("self.loopCount = 0")
        self.fOut.write("self.stateChanged = False")
        if parent_class.statechart.number_time_transitions:
            self.fOut.write()
            self.fOut.write("# Initialize AFTER events")
            self.fOut.write("del self.timers[:]")
            for i in range(parent_class.statechart.number_time_transitions):
                self.fOut.write("self.timers.append(-1)")
        self.fOut.write()
        # write out history state
        if parent_class.statechart.historys:
            self.fOut.write("# History states")
            for node in parent_class.statechart.historyParents:
                self.fOut.write("self.historyState[" + parent_class.name + "." + node.getFullName() + "] = []")
            self.fOut.write()
        # write out initial statechart code
        root = parent_class.statechart.root
        self.fOut.write("# Initial statechart code")
        for c in [root] + parent_class.statechart.composites :
            self.fOut.write("self.currentState[self." + c.getFullName() + "] = []")
        for i in root.defaults:
            if i.isComposite():
                self.fOut.write("self.enterState_" + i.getFullName() + "()")
            elif i.isBasic():
                self.fOut.write("self.enterAction_" + i.getFullName() + "()")
        self.fOut.write()
        self.fOut.dedent()
        
    def exit_Class(self, class_node):    
        # write out str method
        self.fOut.dedent()
        
    #helper method
    def writeMethodSignature(self, name, parameters):
        self.fOut.write("def " + name + "(self")           
        for param in parameters :
            self.fOut.extendWrite(', ')
            param.accept(self)
        self.fOut.extendWrite("):")
        
    #helper method
    def writeMethod(self, name, parameters, return_type, body):
        self.writeMethodSignature(name, parameters)
        self.fOut.indent()
        if body.strip():
            StringUtils.writeCodeCorrectIndent(body, self.fOut)
        else:
            self.fOut.write("return")
        self.fOut.write()
        self.fOut.dedent()
        
    def visit_FormalParameter(self, formal_parameter):
        self.fOut.extendWrite(formal_parameter.getIdent())
        if formal_parameter.hasDefault() :
            self.fOut.extendWrite(" = " + formal_parameter.getDefault())
            
    def visit_FormalEventParameter(self, formal_event_parameter):
        self.fOut.extendWrite(formal_event_parameter.getString())
        
    def visit_Constructor(self, constructor):
        self.fOut.write("#The actual constructor")
        parameters = constructor.getParams() + [SSC.FormalParameter("currentTime", "", "0.0"), SSC.FormalParameter("controller", "", "None"), SSC.FormalParameter("loopMax", "", "1000")]
        self.writeMethodSignature("__init__", parameters)
        self.fOut.indent()
        StringUtils.writeCodeCorrectIndent(constructor.body, self.fOut)
        if constructor.parent_class.statechart is not None :
            self.fOut.write("self.commonConstructor(" + ", ".join([p.getIdent() for p in parameters[-3:]]) +  ")")
        else :
            self.fOut.write("self.commonConstructor()")
        self.fOut.dedent()
        self.fOut.write()
        
    def visit_Destructor(self, destructor):
        self.fOut.write("# User defined destructor")
        self.writeMethod("__del__", [], "", destructor.body)
        
    def visit_Method(self, method):
        self.fOut.write("# User defined method")
        self.writeMethod(method.name, method.parameters, method.type, method.body)
        
        
    #helper method
    def writeTransitionsRecursively(self, current_node):
        self.fOut.write("def transition_" + current_node.getFullName() + "(self, event) :")
        self.fOut.indent()
        
        valid_children = []
        for child in current_node.children :
            if child.isComposite() or child.isBasic() :
                valid_children.append(child)  
         
        self.fOut.write("catched = False")
        
        if current_node.solvesConflictsOuter() :
            self.writeFromTransitions(current_node)
            self.fOut.write("if not catched :")
            self.fOut.indent()
            
        if current_node.isParallel():
            for child in valid_children :     
                self.fOut.write("catched = self.transition_" + child.getFullName() + "(event) or catched")
        elif current_node.isComposite():
            for i, child in enumerate(valid_children) :
                if i > 0 :
                    self.fOut.write("el")
                else :
                    self.fOut.write()
                self.fOut.extendWrite("if self.currentState[self." + current_node.getFullName() + "][0] == self." + child.getFullName() + ":")
                self.fOut.indent()
                self.fOut.write("catched = self.transition_" + child.getFullName() + "(event)")
                self.fOut.dedent()
        else :
            self.fOut.write("pass")
                
        if current_node.solvesConflictsOuter() :
            self.fOut.dedent()
        else :
            self.fOut.write("if not catched :")
            self.fOut.indent()
            self.writeFromTransitions(current_node)
            self.fOut.dedent()
            
        self.fOut.write("return catched")
        self.fOut.dedent()
        
        for child in valid_children :
            self.writeTransitionsRecursively(child)
                
    #helper method
    def writeFromTransitions(self, current_node): 
        # get all transition out of this state
        out_transitions = current_node.getParentStateChart().optimized_transitions[current_node]
        if len(out_transitions) == 0 :
            self.fOut.write("pass")
            return
        
        self.fOut.write('enableds = []')
        for index, transition in enumerate(out_transitions, start=1):
            self.writeTransitionCondition(transition, index)
            
        self.fOut.write("if len(enableds) > 1 :")
        self.fOut.indent()
        self.fOut.write('print "Runtime warning : indeterminism detected in a transition from node ' +  current_node.getFullID()+ '. Only the first in document order enabled transition is executed."')
        self.fOut.dedent()
        self.fOut.write()
        self.fOut.write("if len(enableds) == 1 :")
        self.fOut.indent()
        self.fOut.write('enabled = enableds[0]')      
              
        for index, transition in enumerate(out_transitions, start=1):
            self.writeTransitionAction(transition, index)
        
        self.fOut.write('catched = True')   
        self.fOut.dedent()         
        self.fOut.write()
            
    def visit_BareString(self, bare_string):
        self.fOut.extendWrite(bare_string.string)
        
    def visit_SelfReference(self, self_reference):
        self.fOut.extendWrite("self")
        
    def visit_Target(self, target):
        node = target.getNode()
        self.fOut.extendWrite(node.getParentStateChart().className + "." + node.getFullName())
        
    def enter_InStateCall(self, in_state_call):
        self.fOut.extendWrite("self.inState(")
        
    def exit_InStateCall(self, in_state_call):
        self.fOut.extendWrite(")")
        
    def writeTransitionAction(self, transition, index):
        if index > 1 :
            self.fOut.write("el")
        else :
            self.fOut.write()
        self.fOut.extendWrite("if enabled == " + str(index) + " :")
        self.fOut.indent()
        source_node = transition.parent_node
        statechart = source_node.getParentStateChart()
        # handle parameters to actually use them             
        parameters = transition.getTrigger().getParameters()
        if(len(parameters) > 0) :
            self.fOut.write('parameters = event.getParameters()')
            for index, parameter in enumerate(parameters):
                self.fOut.write()
                parameter.accept(self)
                self.fOut.extendWrite(' = parameters[' + str(index) + ']')
        
        
        target = transition.getTarget().getNode()
        exits, enters = statechart.getTransitionPath(source_node, target)
        
        # write out exit actions
        if not exits[-1].isBasic():
            self.fOut.write("self.exitState_" + exits[-1].getFullName() + "()")
        else:
            for node in exits:
                if node.isBasic():
                    self.fOut.write("self.exitAction_" + node.getFullName() + "()")
                    
        # write out trigger actions
        transition.getAction().accept(self)
        
        # write out enter actions up to second to last entering node
        for i in range(len(enters) - 1):
            if enters[i].isComposite():
                self.fOut.write("self.enterAction_" + enters[i].getFullName() + "()")
                if enters[i+1].isOrthogonal():
                    stateName = ""
                    for j in statechart.defaults[enters[i]]:
                        if enters[i+1] != j:
                            self.fOut.write("self.enterState_" + j.getFullName() + "()")
                        stateName = stateName + ", self." + j.getFullName()
                    stateName = "[" + stateName[2:] + "]"
                    self.fOut.write("self.currentState[self." + enters[i].getFullName() + "] = " + stateName)
        # based on the last entering node, we have special functions we can call
        if enters[-1].isComposite():
            self.fOut.write("self.enterState_" + enters[-1].getFullName() + "()")
        elif enters[-1].isHistory():
            self.fOut.write("self.enterHistory_" + enters[-1].getParentNode().getFullName() + "(" + str(enters[-1].isHistoryDeep()) + ")")
        else:
            self.fOut.write("self.enterAction_" + enters[-1].getFullName() + "()")
        self.fOut.write('catched = True')
        self.fOut.dedent()
                        
    def writeTransitionCondition(self, transition, index):
        trigger = transition.getTrigger()
        if not trigger.isUC():  
            self.fOut.write('if event.getName() == "' + trigger.getEvent() + '" and event.getPort() == "' + trigger.getPort() + '" :')
            self.fOut.indent()   
        # evaluate guard
        if transition.hasGuard() :   
            # handle parameters for guard evaluation       
            parameters = trigger.getParameters();
            if(len(parameters) > 0) :
                self.fOut.write('parameters = event.getParameters()')
                for i, parameter in enumerate(parameters):
                    self.fOut.write()
                    parameter.accept(self)
                    self.fOut.extendWrite(' = parameters[' + str(i) + ']')   

            self.fOut.write('if ')
            transition.getGuard().accept(self)
            self.fOut.extendWrite(' :')
            self.fOut.indent()    
            
        self.fOut.write("enableds.append(" + str(index) + ")")

        if transition.hasGuard() :
            self.fOut.dedent()
        if not trigger.isUC() :
            self.fOut.dedent()
        self.fOut.write()
    
    def visit_EnterAction(self, enter_method):
        parent_node = enter_method.parent_node
        statechart = parent_node.getParentStateChart()
        self.writeMethodSignature("enterAction_" + parent_node.getFullName(), [])
        self.fOut.indent()
        # take care of any AFTER events
        timers = statechart.afterNodeEvents[parent_node]
        if timers:
            for key, value in timers:
                self.fOut.write("self.timers[" + str(key) + "] = " + value + " + self.currentTime")
        if enter_method.action:
            enter_method.action.accept(self)
        self.fOut.write("self.currentState[" + statechart.className + "." + parent_node.getParentNode().getFullName() + "].append(" + statechart.className + "." + parent_node.getFullName() + ")")
        self.fOut.dedent()
        self.fOut.write()
         
    def visit_ExitAction(self, exit_method):
        parent_node = exit_method.parent_node
        statechart = parent_node.getParentStateChart()
        self.writeMethodSignature("exitAction_" + parent_node.getFullName(), [])
        self.fOut.indent()
        # take care of any AFTER events
        timers = statechart.afterNodeEvents[parent_node]
        if timers:
            for timer in timers:
                self.fOut.write("self.timers[" + str(timer[0]) + "] = -1")
        if exit_method.action:
            exit_method.action.accept(self)
        self.fOut.write("self.currentState[" + statechart.className + "." + parent_node.getParentNode().getFullName() + "] = []")
        self.fOut.dedent()
        self.fOut.write()
        
    #helper method
    def writeEnterState(self, entered_node):
        self.writeMethodSignature("enterState_" + entered_node.getFullName(), [])
        self.fOut.indent()
        if entered_node.isComposite():
            self.fOut.write("self.enterAction_" + entered_node.getFullName() + "()")
        l = entered_node.getDefaults()
        for i in l:
            if i.isComposite():
                self.fOut.write("self.enterState_" + i.getFullName() + "()")
            elif i.isBasic():
                self.fOut.write("self.enterAction_" + i.getFullName() + "()")
        self.fOut.dedent()
      
    #helper method            
    def writeExitState(self, exited_node):
        self.writeMethodSignature("exitState_" + exited_node.getFullName(), [])
        self.fOut.indent()
        class_name = exited_node.parent_statechart.className
        if exited_node in exited_node.parent_statechart.historyParents:
            self.fOut.write("self.historyState[" + class_name + "." + exited_node.getFullName() + "] = " \
              + "self.currentState[" + class_name + "." + exited_node.getFullName() + "]")
            
        l = exited_node.getChildren()
        if exited_node.isParallel():
            for thing in l:
                if not thing.isHistory() :
                    self.fOut.write("self.exitState_" + thing.getFullName() + "()")
            #self.fOut.write("self.currentState[" + class_name + "." + exited_node.getFullName() + "] = []")
        else:
            for thing in l:
                if not thing.isHistory() :
                    self.fOut.write("if " + class_name + "." + thing.getFullName() +  " in self.currentState[" + class_name + "." + exited_node.getFullName() + "] :")
                    self.fOut.indent()
                    if thing.isComposite() :     
                        self.fOut.write("self.exitState_" + thing.getFullName() + "()")
                    else:
                        self.fOut.write("self.exitAction_" + thing.getFullName() + "()")
                    self.fOut.dedent()  
        #if not exited_node.isOrthogonal() :
        self.fOut.write("self.exitAction_" + exited_node.getFullName() + "()")
        self.fOut.dedent()
            
    #helper method
    def writeEnterHistory(self, entered_node):
        self.writeMethodSignature("enterHistory_" + entered_node.getFullName(), [SSC.FormalParameter("deep","")])
        self.fOut.indent()
        class_name = entered_node.parent_statechart.className
        self.fOut.write("if self.historyState[" + class_name + "." + entered_node.getFullName() + "] == []:")
        self.fOut.indent()
        l = entered_node.getDefaults()
        stateName = ""
        for i in l:
            self.fOut.write("self.enterState_" + i.getFullName() + "()")
            stateName = stateName + ", " + class_name + "." + i.getFullName()
        stateName = "[" + stateName[2:] + "]"
        self.fOut.write("self.currentState[" + class_name + "." + entered_node.getFullName() + "] = " + stateName)

        self.fOut.dedent()
        self.fOut.write("else:")
        self.fOut.indent()
        l = entered_node.getChildren()
        if entered_node.isParallel():
            stateNames = []
            for i in l:
                if not i.isHistory() :
                    self.fOut.write("self.enterHistory_" + i.getFullName() + "(deep)")
                    stateNames.append(class_name + "." + i.getFullName())
            self.fOut.write("self.currentState[" + class_name + "." + entered_node.getFullName() + "] = [" + ", ".join(stateNames) +"]")
        else:
            for thing in l:
                if not thing.isHistory() :
                    self.fOut.write("if " +  class_name + "." + thing.getFullName() + " in self.historyState[" + class_name + "." + entered_node.getFullName() + "] :")
                    self.fOut.indent()
                    if thing.isComposite():
                        self.fOut.write("if deep:")
                        self.fOut.indent()
                        self.fOut.write("self.enterAction_" + thing.getFullName() + "()")
                        self.fOut.write("self.enterHistory_" + thing.getFullName() + "(deep)")
                        self.fOut.dedent()
                        self.fOut.write("else:")
                        self.fOut.indent()
                        self.fOut.write("self.enterState_" + thing.getFullName() + "()")
                        self.fOut.dedent()
                    else:
                        self.fOut.write("self.enterAction_" + thing.getFullName() + "()")
                    self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.dedent()

    def enter_StateChart(self, statechart):
        self.fOut.write("# First statechart enter/exit action methods")
                
    def exit_StateChart(self, statechart): 
        # write out statecharts methods for enter/exit state
        if statechart.composites :
            self.fOut.write("# Statechart enter/exit state methods")
            for i in statechart.composites :
                self.writeEnterState(i)
                self.writeExitState(i)
        # write out statecharts methods for enter/exit history
        if statechart.historys:
            self.fOut.write("# Statechart enter/exit history methods")
            for i in statechart.historyParents:
                self.writeEnterHistory(i)     
                

        self.writeTransitionsRecursively(statechart.root)            
                
        # write out transition function
        self.fOut.write("# Execute transitions")
        self.fOut.write("def transition(self, event):")
        self.fOut.indent()
        self.fOut.write()
        self.fOut.write("self.stateChanged = self.transition_" + statechart.root.getFullName() + "(event)")
        self.fOut.dedent()

        # write out microstep function
        self.fOut.write("# Execute microstep")
        self.fOut.write("def microstep(self):")
        self.fOut.indent()
        self.fOut.write("if self.eventQueue:")
        self.fOut.indent()
        self.fOut.write("currentEvents = []")
        self.fOut.write("laterEvents = []")
        self.fOut.write("for i in self.eventQueue:")
        self.fOut.indent()
        self.fOut.write("if i.getTime() <= self.currentTime:")
        self.fOut.indent()
        self.fOut.write("currentEvents.append(i)")
        self.fOut.dedent()
        self.fOut.write("else:")
        self.fOut.indent()
        self.fOut.write("laterEvents.append(i)")
        self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.write("self.eventQueue = laterEvents")
        self.fOut.write("while currentEvents:")
        self.fOut.indent()
        self.fOut.write("current = currentEvents.pop(0)")
        self.fOut.write("self.transition(current)")
        self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.write("else:")
        self.fOut.indent()
        self.fOut.write("self.transition(Event(\"\"))")
        self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.write()

        # write out step function
        self.fOut.write("# Execute statechart")
        self.fOut.write("def step(self, currentTime):")
        self.fOut.indent()
        self.fOut.write("self.currentTime = currentTime")
        self.fOut.write()
        if statechart.number_time_transitions != 0:
            self.fOut.write("# Check AFTER timers")
            self.fOut.write("for i in range(len(self.timers)):")
            self.fOut.indent()
            self.fOut.write("if self.timers[i] > 0 and self.timers[i] <= self.currentTime:")
            self.fOut.indent()
            self.fOut.write("self.event(Event(\"_\" + str(i) + \"after\", self.timers[i]))")
            self.fOut.dedent()
            self.fOut.dedent()
            self.fOut.write()
        self.fOut.write("self.microstep()")
        self.fOut.write("self.loopCount = 0")
        self.fOut.write("while self.stateChanged:")
        self.fOut.indent()
        self.fOut.write("self.loopCount += 1")
        self.fOut.write("if self.loopCount >= self.loopMax:")
        self.fOut.indent()
        self.fOut.write("print \"Runtime Error: \", \"Infinite loop detected in class <" + statechart.className + ">. Aborting...\"")
        self.fOut.write("sys.exit(1)")
        self.fOut.dedent()
        self.fOut.write("if self.getEarliestEvent() <= self.currentTime:")
        self.fOut.indent()
        self.fOut.write("self.microstep()")
        self.fOut.dedent()
        self.fOut.write("else:")
        self.fOut.indent()
        self.fOut.write("self.transition(Event(\"\"))")
        self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.write()

        # write out inState function
        self.fOut.write("# inState method for statechart")
        self.fOut.write("def inState(self, state):")
        self.fOut.indent()
        self.fOut.write("for actives in self.currentState.itervalues():")
        self.fOut.indent()
        self.fOut.write("if state in actives:")
        self.fOut.indent()
        self.fOut.write("return True")
        self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.write("return False")
        self.fOut.dedent()
        self.fOut.write()

        # write out event method
        self.fOut.write("# Event method")
        self.fOut.write("def event(self, newEvent):")
        self.fOut.indent()
        self.fOut.write("if newEvent.getTime() < self.currentTime:")
        self.fOut.indent()
        self.fOut.write("print \"Runtime Warning: Cannot add event with negative time!\"")
        self.fOut.write("return")
        self.fOut.dedent()
        self.fOut.write("self.eventQueue.append(newEvent)")
        self.fOut.dedent()
        self.fOut.write()

        # write out getEarliestEvent method
        self.fOut.write("# Get earliest event method")
        self.fOut.write("def getEarliestEvent(self):")
        self.fOut.indent()
        self.fOut.write("temp = []")
        self.fOut.write("for j in self.eventQueue:")
        self.fOut.indent()
        self.fOut.write("temp.append(j.getTime())")
        self.fOut.dedent()
        if statechart.number_time_transitions:
            self.fOut.write("for j in self.timers:")
            self.fOut.indent()
            self.fOut.write("if j > -1:")
            self.fOut.indent()
            self.fOut.write("temp.append(j)")
            self.fOut.dedent()
            self.fOut.dedent()
        self.fOut.write("if temp:")
        self.fOut.indent()
        self.fOut.write("temp.sort()")
        self.fOut.write("return temp[0]")
        self.fOut.dedent()
        self.fOut.write("else:")
        self.fOut.indent()
        self.fOut.write("return None")
        self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.write()
        
        # write out addAssociatedObject method
        self.fOut.write("def addAssociatedObject(self, name, instance):")
        self.fOut.indent()
        self.fOut.write("if(name in self.associates):")
        self.fOut.indent()
        self.fOut.write('print "Runtime Warning: Instance name already present! Delete first or change name."')
        self.fOut.dedent()
        self.fOut.write("else :")
        self.fOut.indent()
        self.fOut.write("self.associates[name] = instance")
        self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.write()
          
        # write out deleteAssociatedObject method  
        self.fOut.write("def deleteAssociatedObject(self, name):")
        self.fOut.indent()
        self.fOut.write("if(name in self.associates):")
        self.fOut.indent()
        self.fOut.write("del self.associates[name]")
        self.fOut.dedent()
        self.fOut.write("else :")
        self.fOut.indent()
        self.fOut.write('print "Runtime Warning: Tried to delete an instance that doesn\'t exist."')
        self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.write()
        
        # write out dump method
        self.fOut.write("# Dump method")
        self.fOut.write("def dump(self, toDump):")
        self.fOut.indent()
        self.fOut.write()
        self.fOut.write("print toDump")
        self.fOut.dedent()
        self.fOut.write()
        
    def visit_Association(self, association):      
        self.fOut.write('self.associations_info["' + association.from_class + '"].append(')
        self.fOut.extendWrite('AssociationInfo("' + association.to_class + '", ' + str(association.min) + ', ')
        if association.max == 'N' :
            self.fOut.extendWrite('"N"')
        else :
            self.fOut.extendWrite(str(association.max))
        self.fOut.extendWrite('))')
        
    def visit_RaiseEvent(self, raise_event):
        if raise_event.isLocal():
            self.fOut.write('self.event(Event("' + raise_event.getEventName() +'", time = self.currentTime, parameters = [')
        elif raise_event.isNarrow():
            self.fOut.write('if("' + raise_event.getTarget() + '" in self.associates):')
            self.fOut.indent()
            self.fOut.write('self.associates["' + raise_event.getTarget() + '"].event(Event("'+ raise_event.getEventName() +'", time = self.currentTime, parameters = [')
        elif raise_event.isBroad():
            self.fOut.write('self.controller.broadcast(Event("' + raise_event.getEventName() +'", time = self.currentTime, parameters = [')
        elif raise_event.isOutput():
            self.fOut.write('self.controller.outputEvent(Event("' + raise_event.getEventName() + '", time = self.currentTime, port="' + raise_event.getPort() + '", parameters = [')
        first_param = True
        for param in raise_event.getParameters() :
            if first_param :
                first_param = False
            else :
                self.fOut.extendWrite(',')
            param.accept(self)
        self.fOut.extendWrite(']))')
        if raise_event.isNarrow():
            self.fOut.dedent()
            self.fOut.write("else :")
            self.fOut.indent()
            self.fOut.write('print "Runtime Warning: Tried to do a narrow cast to an unknown instance name."')
            self.fOut.dedent()
            
    def visit_Script(self, script):
        StringUtils.writeCodeCorrectIndent(script.code, self.fOut)
        
    def visit_Log(self, log):
        self.fOut.write('print "' + log.message + '"')
        
    def visit_AddAssociatedObject(self, add):
        self.fOut.write('self.addAssociatedObject("' + add.name +'", ' + add.reference + ')')
        
    def visit_Assign(self, assign):
        self.fOut.write()
        assign.lvalue.accept(self)
        self.fOut.extendWrite(" = ")
        assign.expression.accept(self)
    