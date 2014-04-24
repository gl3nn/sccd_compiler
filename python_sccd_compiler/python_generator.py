import utils as StringUtils
import time
from constructs import FormalParameter
from code_generation import CodeGenerator, Protocols

class PythonGenerator(CodeGenerator):
    
    def __init__(self, class_diagram, output_file, protocol):
        super(PythonGenerator,self).__init__(class_diagram, output_file, protocol)
        self.supported_protocols = [Protocols.Threads, Protocols.GameLoop]
                
    def visit_ClassDiagram(self, class_diagram):
        self.fOut.write("# Statechart compiler by Glenn De Jonghe")
        self.fOut.write("#")
        self.fOut.write("# Source: " + class_diagram.source)
        self.fOut.write("# Date:   " + time.asctime())
        if class_diagram.name or class_diagram.author or class_diagram.description:
            self.fOut.write()
        if class_diagram.author:
            self.fOut.write("# Model author: " + class_diagram.author)
        if class_diagram.name:
            self.fOut.write("# Model name:   " + class_diagram.name)
        if class_diagram.description.strip():
            self.fOut.write("# Model description:")
            self.fOut.write('"""')
            self.fOut.indent()
            self.fOut.write(class_diagram.description.strip())
            self.fOut.dedent()
            self.fOut.write('"""')
        self.fOut.write()
        
        #Mandatory imports
        self.fOut.write('from python_runtime.statecharts_core import ObjectManagerBase, Event, InstanceWrapper, RuntimeClassBase, Association')
        #User imports
        if class_diagram.top.strip():
            StringUtils.writeCodeCorrectIndent(class_diagram.top, self.fOut)
        self.fOut.write()
        
        #visit children
        for c in class_diagram.classes :
            c.accept(self)
         
        #writing out ObjectManager
        self.fOut.write('class ObjectManager (ObjectManagerBase):')
        self.fOut.indent()
        self.fOut.write('def __init__(self, controller):')
        self.fOut.indent()
        self.fOut.write("super(ObjectManager, self).__init__(controller)")
        self.fOut.dedent()
        self.fOut.write()
        
        self.fOut.write('def instantiate(self, class_name, construct_params):')
        self.fOut.indent()
        self.fOut.write("associations = []")
        for index, c in enumerate(class_diagram.classes) :
            if index == 0 : 
                self.fOut.write()
            else :
                self.fOut.write('el')
            self.fOut.extendWrite('if class_name == "' + c.name + '" :')
            self.fOut.indent()
            if c.statechart :
                self.fOut.write('instance =  ' + c.name + '(self.controller, *construct_params)')
            else :
                self.fOut.write('instance =  ' + c.name + '(*construct_params)')
            for a in c.associations :
                a.accept(self)
            self.fOut.dedent()
        self.fOut.write('if instance:')
        self.fOut.indent()
        self.fOut.write('return InstanceWrapper(instance, associations)')
        self.fOut.dedent()
        self.fOut.write('else :')
        self.fOut.indent()
        self.fOut.write('return None')
        self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.dedent()
        
        self.fOut.write()
        if self.protocol == Protocols.Threads :
            controller_sub_class = "ThreadsControllerBase"
        elif self.protocol == Protocols.GameLoop :
            controller_sub_class = "GameLoopControllerBase"
        self.fOut.write("from python_runtime.statecharts_core import " + controller_sub_class)

        # write out controller
        self.fOut.write("class Controller(" + controller_sub_class + "):")
        self.fOut.indent()
    
        # write out __init__ method
        if class_diagram.default_class.constructors :
            for constructor in class_diagram.default_class.constructors :
                self.writeControllerConstructor(class_diagram, constructor.parameters)
        else :
            self.writeControllerConstructor(class_diagram)

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
        
    #helper method
    def writeControllerConstructor(self, class_diagram, parameters = []):
        self.writeMethodSignature('__init__', parameters + [FormalParameter("keep_running", "", "True")])
        self.fOut.indent()
        self.fOut.write("super(Controller, self).__init__(ObjectManager(self), keep_running)")
        for i in class_diagram.inports:
            self.fOut.write('self.addInputPort("' + i + '")')
        for i in class_diagram.outports:
            self.fOut.write('self.addOutputPort("' + i + '")')
        actual_parameters = [p.getIdent() for p in parameters]
        self.fOut.write('self.object_manager.createInstance("'+ class_diagram.default_class.name +'", [' +  ', '.join(actual_parameters)+ '])')
        self.fOut.write()
        self.fOut.dedent()

    def visit_Class(self, class_node):
        """
        Generate code for Class construct
        """
        self.fOut.write()
        # take care of inheritance
        if class_node.super_classes:
            super_classes = []
            for super_class in class_node.super_classes:
                super_classes.append(super_class)
            self.fOut.write("class " + class_node.name + "(" + ", ".join(super_classes) +  "):")
        else:
            self.fOut.write("class " + class_node.name + "(RuntimeClassBase):")

        self.fOut.indent()
        self.fOut.write()
        
        if class_node.statechart is not None:
            # assign each node a unique ID
            self.fOut.write("# Unique IDs for all statechart nodes")
            for (i,node) in enumerate(class_node.statechart.composites + class_node.statechart.basics):
                self.fOut.write(node.getFullName() + " = " + str(i))
    
            self.fOut.write()
            self.writeMethodSignature("commonConstructor", [FormalParameter("controller", "", "None")])
        else :
            self.writeMethodSignature("commonConstructor")
        self.fOut.indent()
        self.fOut.write('"""Constructor part that is common for all constructors."""')
        self.fOut.write("super(" + class_node.name + ", self).__init__()")

        # write attributes
        if class_node.attributes:
            self.fOut.write("# User defined attributes")
            for attribute in class_node.attributes:
                if attribute.init_value is None :
                    self.fOut.write("self." +  attribute.name + " = None")
                else :
                    self.fOut.write("self." +  attribute.name + " = " + attribute.init_value)     
            self.fOut.write()

        # if there is a statechart
        if class_node.statechart is not None:            
            self.fOut.write("self.controller = controller")
            self.fOut.write("self.object_manager = controller.getObjectManager()")
            self.fOut.write("self.current_state = {}")
            self.fOut.write("self.history_state = {}")
            if class_node.statechart.nr_of_after_transitions:
                self.fOut.write("self.timers = {}")
            self.fOut.write()
            self.fOut.write("#Initialize statechart :")
            self.fOut.write()
            
            if class_node.statechart.historys:
                for node in class_node.statechart.combined_history_parents:
                    self.fOut.write("self.history_state[" + class_node.name + "." + node.getFullName() + "] = []")
                self.fOut.write()

            for c in class_node.statechart.composites :
                self.fOut.write("self.current_state[self." + c.getFullName() + "] = []")
        
        self.fOut.dedent()
        self.fOut.write()
        
        self.writeMethodSignature("start")
        self.fOut.indent()
        self.fOut.write("super(" + class_node.name + ", self).start()")
        for default_node in class_node.statechart.root.defaults:
            if default_node.isComposite():
                self.fOut.write("self.enterDefault_" + default_node.getFullName() + "()")
            elif default_node.isBasic():
                self.fOut.write("self.enter_" + default_node.getFullName() + "()")
        self.fOut.dedent()
        self.fOut.write()
        
        #visit children
        for i in class_node.constructors :
            i.accept(self)
        for i in class_node.destructors :
            i.accept(self)
        for i in class_node.methods :
            i.accept(self)
        if class_node.statechart is not None:
            class_node.statechart.accept(self)
          
        # write out str method
        self.fOut.dedent()

    #helper method
    def writeMethodSignature(self, name, parameters = []):
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
        
    def visit_Constructor(self, constructor):
        self.fOut.write("#The actual constructor")
        parameters =  [FormalParameter("controller", "", None)] + constructor.getParams()
        self.writeMethodSignature("__init__", parameters)
        self.fOut.indent()
        if constructor.parent_class.statechart is not None :
            self.fOut.write("self.commonConstructor(controller)")
        else :
            self.fOut.write("self.commonConstructor()")
        if constructor.body :
            self.fOut.write()
            self.fOut.write("#constructor body (user-defined)")
            StringUtils.writeCodeCorrectIndent(constructor.body, self.fOut)
        self.fOut.dedent()
        self.fOut.write()
        
    def visit_Destructor(self, destructor):
        self.fOut.write("# User defined destructor")
        self.writeMethod("__del__", [], "", destructor.body)
        
    def visit_Method(self, method):
        self.fOut.write("# User defined method")
        self.writeMethod(method.name, method.parameters, method.return_type, method.body)
        
    def visit_Association(self, association):
        self.fOut.write('associations.append(Association("' + association.name + '", "' + association.to_class + '", ' + str(association.min) + ', ' + str(association.max) + '))')
        
    #helper method
    def writeTransitionsRecursively(self, current_node):
        self.fOut.write("def transition_" + current_node.getFullName() + "(self, event) :")
        self.fOut.indent()
        
        valid_children = []
        for child in current_node.children :
            if child.isComposite() or child.isBasic() :
                valid_children.append(child)  
         
        self.fOut.write("catched = False")
        do_dedent = False
        if current_node.solvesConflictsOuter() :
            self.writeFromTransitions(current_node)
            if current_node.isParallel() or current_node.isComposite() :
                self.fOut.write("if not catched :")
                self.fOut.indent()
                do_dedent = True
            
        if current_node.isParallel():
            for child in valid_children :     
                self.fOut.write("catched = self.transition_" + child.getFullName() + "(event) or catched")
        elif current_node.isComposite():
            for i, child in enumerate(valid_children) :
                if i > 0 :
                    self.fOut.write("el")
                else :
                    self.fOut.write()
                self.fOut.extendWrite("if self.current_state[self." + current_node.getFullName() + "][0] == self." + child.getFullName() + ":")
                self.fOut.indent()
                self.fOut.write("catched = self.transition_" + child.getFullName() + "(event)")
                self.fOut.dedent()
                
        if current_node.solvesConflictsOuter() :
            if do_dedent :
                self.fOut.dedent()
        elif len(current_node.getTransitions()) > 0 :
                self.fOut.write("if not catched :")
                self.fOut.indent()
                self.writeFromTransitions(current_node)
                self.fOut.dedent()
            
        self.fOut.write("return catched")
        self.fOut.dedent()
        self.fOut.write();
        
        for child in valid_children :
            self.writeTransitionsRecursively(child)
                
    #helper method
    def writeFromTransitions(self, current_node): 
        # get all transition out of this state
        out_transitions = current_node.getTransitions()
        if len(out_transitions) == 0 :
            return
        
        self.fOut.write('enableds = []')
        for index, transition in enumerate(out_transitions, start=1):
            self.writeTransitionCondition(transition, index)
            
        self.fOut.write("if len(enableds) > 1 :")
        self.fOut.indent()
        self.fOut.write('print "Runtime warning : indeterminism detected in a transition from node ' +  current_node.getFullID()+ '. Only the first in document order enabled transition is executed."')
        self.fOut.dedent()
        self.fOut.write()
        self.fOut.write("if len(enableds) > 0 :")
        self.fOut.indent()
        self.fOut.write('enabled = enableds[0]')      
              
        for index, transition in enumerate(out_transitions, start=1):
            self.writeTransitionAction(transition, index)
        
        self.fOut.write('catched = True')   
        self.fOut.dedent()         
        self.fOut.write()
        
    def visit_FormalEventParameter(self, formal_event_parameter):
        self.fOut.extendWrite(formal_event_parameter.getName())
        
    def writeFormalEventParameters(self, transition):
        parameters = transition.getTrigger().getParameters()
        if(len(parameters) > 0) :
            self.fOut.write('parameters = event.getParameters()')
            for index, parameter in enumerate(parameters):
                self.fOut.write()
                parameter.accept(self)
                self.fOut.extendWrite(' = parameters[' + str(index) + ']')
        
        
    def writeTransitionAction(self, transition, index):
        if index > 1 :
            self.fOut.write("el")
        else :
            self.fOut.write()
        self.fOut.extendWrite("if enabled == " + str(index) + " :")
        self.fOut.indent()

        # handle parameters to actually use them             
        self.writeFormalEventParameters(transition)
        
        exits = transition.getExitNodes()
        
        # write out exit actions
        if not exits[-1].isBasic():
            self.fOut.write("self.exit_" + exits[-1].getFullName() + "()")
        else:
            for node in exits:
                if node.isBasic():
                    self.fOut.write("self.exit_" + node.getFullName() + "()")
                    
        # write out trigger actions
        transition.getAction().accept(self)
        
        for (entering_node, is_ending_node) in transition.getEnterNodes() : 
            if is_ending_node :
                if entering_node.isComposite():
                    self.fOut.write("self.enterDefault_" + entering_node.getFullName() + "()")
                elif entering_node.isHistory():
                    if (entering_node.isHistoryDeep()) :
                        self.fOut.write("self.enterHistoryDeep_" + entering_node.getParentNode().getFullName() + "()")
                    else :
                        self.fOut.write("self.enterHistoryShallow_" + entering_node.getParentNode().getFullName() + "()")
                else:
                    self.fOut.write("self.enter_" + entering_node.getFullName() + "()")
            else :
                if entering_node.isComposite():
                    self.fOut.write("self.enter_" + entering_node.getFullName() + "()")

        self.fOut.dedent()
                        
    def writeTransitionCondition(self, transition, index):
        trigger = transition.getTrigger()
        if not trigger.isUC():  
            self.fOut.write('if event.getName() == "' + trigger.getEvent() + '" and event.getPort() == "' + trigger.getPort() + '" :')
            self.fOut.indent()   
        # evaluate guard
        if transition.hasGuard() :   
            # handle parameters for guard evaluation       
            self.writeFormalEventParameters(transition)

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
        self.writeMethodSignature("enter_" + parent_node.getFullName(), [])
        self.fOut.indent()
        # take care of any AFTER events
        for transition in parent_node.transitions :
            trigger = transition.getTrigger()
            if trigger.isAfter() :
                self.fOut.write("self.timers[" + str(trigger.getAfterIndex()) + "] = ")
                trigger.after.accept(self)
        if enter_method.action:
            enter_method.action.accept(self)
        self.fOut.write("self.current_state[self." + parent_node.getParentNode().getFullName() + "].append(self." + parent_node.getFullName() + ")")
        self.fOut.dedent()
        self.fOut.write()
        
    #helper method
    def writeEnterDefault(self, entered_node):
        self.writeMethodSignature("enterDefault_" + entered_node.getFullName(), [])
        self.fOut.indent()
        self.fOut.write("self.enter_" + entered_node.getFullName() + "()")
        if entered_node.isComposite():
            l = entered_node.getDefaults()
            for i in l:
                if i.isComposite():
                    self.fOut.write("self.enterDefault_" + i.getFullName() + "()")
                elif i.isBasic():
                    self.fOut.write("self.enter_" + i.getFullName() + "()")
        self.fOut.dedent()
        self.fOut.write()
         
    def visit_ExitAction(self, exit_method):
        exited_node = exit_method.parent_node
        self.writeMethodSignature("exit_" + exited_node.getFullName(), [])
        self.fOut.indent()
        
        #If the exited node is composite take care of potential history and the leaving of descendants
        if exited_node.isComposite() :
            #handle history
            if exited_node.save_state_on_exit :
                self.fOut.write("self.history_state[self." + exited_node.getFullName() + "] = self.current_state[self." + exited_node.getFullName() + "]")
            
            #Take care of leaving children
            children = exited_node.getChildren()
            if exited_node.isParallel():
                for child in children:
                    if not child.isHistory() :
                        self.fOut.write("self.exit_" + child.getFullName() + "()")
            else:
                for child in children:
                    if not child.isHistory() :
                        self.fOut.write("if self." + child.getFullName() +  " in self.current_state[self." + exited_node.getFullName() + "] :")
                        self.fOut.indent()
                        self.fOut.write("self.exit_" + child.getFullName() + "()")
                        self.fOut.dedent()  
        
        
        # take care of any AFTER events
        for transition in exited_node.transitions :
            trigger = transition.getTrigger()
            if trigger.isAfter() :
                self.fOut.write("self.timers.pop(" + str(trigger.getAfterIndex()) + ", None)")
                
        #Execute user-defined exit action if present
        if exit_method.action:
            exit_method.action.accept(self)
            
        #Adjust state
        self.fOut.write("self.current_state[self." + exited_node.getParentNode().getFullName() + "] = []") # SPECIAL CASE FOR ORTHOGONAL??
        
        self.fOut.dedent()
        self.fOut.write()
        
            
    #helper method
    def writeEnterHistory(self, entered_node, is_deep):
        self.writeMethodSignature("enterHistory" + ("Deep" if is_deep else "Shallow") + "_" + entered_node.getFullName(), [])
        self.fOut.indent()
        self.fOut.write("if self.history_state[self." + entered_node.getFullName() + "] == []:")
        self.fOut.indent()
        defaults = entered_node.getDefaults()

        for node in defaults:
            if node.isBasic() :
                self.fOut.write("self.enter_" + node.getFullName() + "()")
            elif node.isComposite() :
                self.fOut.write("self.enterDefault_" + node.getFullName() + "()")

        self.fOut.dedent()
        self.fOut.write("else:")
        self.fOut.indent()
        children = entered_node.getChildren()
        if entered_node.isParallel():
            for child in children:
                if not child.isHistory() :
                    self.fOut.write("self.enterHistory" + ("Deep" if is_deep else "Shallow") + "_" + child.getFullName() + "()")
        else:
            for child in children:
                if not child.isHistory() :
                    self.fOut.write("if self." + child.getFullName() + " in self.history_state[self." + entered_node.getFullName() + "] :")
                    self.fOut.indent()
                    if child.isComposite():
                        if is_deep :
                            self.fOut.write("self.enter_" + child.getFullName() + "()")
                            self.fOut.write("self.enterHistoryDeep_" + child.getFullName() + "()")
                        else :
                            self.fOut.write("self.enterDefault_" + child.getFullName() + "()")
                    else:
                        self.fOut.write("self.enter_" + child.getFullName() + "()")
                    self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.write()

    def visit_StateChart(self, statechart):
        self.fOut.write("# Statechart enter/exit action method(s) :")
        self.fOut.write()
        
        #visit children
        for i in statechart.composites + statechart.basics:
            if i is not statechart.root :
                i.getEnterAction().accept(self)
                i.getExitAction().accept(self)

        # write out statecharts methods for enter/exit state
        if len(statechart.composites) > 1 :
            self.fOut.write("#Statechart enter/exit default method(s) :")
            self.fOut.write()
            for i in statechart.composites :
                if i is not statechart.root :
                    self.writeEnterDefault(i)

        # write out statecharts methods for enter/exit history
        if statechart.historys:
            self.fOut.write("#Statechart enter/exit history method(s) :")
            self.fOut.write()
            for i in statechart.shallow_history_parents:
                self.writeEnterHistory(i, False)
            for i in statechart.deep_history_parents:
                self.writeEnterHistory(i, True) 
           
           
        self.fOut.write("#Statechart transitions :")     
        self.fOut.write()
        self.writeTransitionsRecursively(statechart.root)            
                
        # write out transition function
        self.fOut.write("# Execute transitions")
        self.fOut.write("def transition(self, event = Event(\"\")):")
        self.fOut.indent()
        self.fOut.write("self.state_changed = self.transition_" + statechart.root.getFullName() + "(event)")
        self.fOut.dedent()

        # write out inState function
        self.fOut.write("# inState method for statechart")
        self.fOut.write("def inState(self, nodes):")
        self.fOut.indent()
        self.fOut.write("for actives in self.current_state.itervalues():")
        self.fOut.indent()
        self.fOut.write("nodes = [node for node in nodes if node not in actives]")
        self.fOut.write("if not nodes :")
        self.fOut.indent()
        self.fOut.write("return True")
        self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.write("return False")
        self.fOut.dedent()
        self.fOut.write()
        
    def visit_ExpressionPartString(self, bare_string):
        self.fOut.extendWrite(bare_string.string)
        
    def visit_SelfReference(self, self_reference):
        self.fOut.extendWrite("self")
        
    def visit_StateReference(self, state_ref):
        self.fOut.extendWrite("[" + ",".join(["self." + node.getFullName() for node in state_ref.getNodes()]) + "]")
        
    def visit_InStateCall(self, in_state_call):
        self.fOut.extendWrite("self.inState(")
        in_state_call.target.accept(self)
        self.fOut.extendWrite(")")
        
    def visit_RaiseEvent(self, raise_event):
        if raise_event.isNarrow() or raise_event.isBroad():
            self.fOut.write('send_event = Event("' + raise_event.getEventName() + '", parameters = [')
        elif raise_event.isLocal():
            self.fOut.write('self.addEvent(Event("' + raise_event.getEventName() +'", parameters = [')
        elif raise_event.isOutput():
            self.fOut.write('self.controller.outputEvent(Event("' + raise_event.getEventName() + '", port="' + raise_event.getPort() + '", parameters = [')
        elif raise_event.isCD():
            self.fOut.write('self.object_manager.addEvent(Event("' + raise_event.getEventName() + '", parameters = [self, ')
        first_param = True
        for param in raise_event.getParameters() :
            if first_param :
                first_param = False
            else :
                self.fOut.extendWrite(',')
            param.accept(self)
        if raise_event.isNarrow():
            self.fOut.extendWrite('])')
            self.fOut.write('self.object_manager.addEvent(Event("narrow_cast", parameters = [self, "' + raise_event.getTarget() + '" , send_event]))')
        elif raise_event.isBroad():
            self.fOut.extendWrite('])')
            self.fOut.write('self.object_manager.addEvent(Event("broad_cast", parameters = [send_event]))')
        else :
            self.fOut.extendWrite(']))')
            
    def visit_Script(self, script):
        StringUtils.writeCodeCorrectIndent(script.code, self.fOut)
        
    def visit_Log(self, log):
        self.fOut.write('print "' + log.message + '"')
        
    def visit_Assign(self, assign):
        self.fOut.write()
        assign.lvalue.accept(self)
        self.fOut.extendWrite(" = ")
        assign.expression.accept(self)
    