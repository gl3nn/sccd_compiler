"""Generates C#"""

import utils as StringUtils
import time
from constructs import FormalParameter
from code_generation import CodeGenerator, Protocols

class CSharpGenerator(CodeGenerator):
    
    def __init__(self, class_diagram, output_file, protocol):
        super(CSharpGenerator,self).__init__(class_diagram, output_file, protocol)
        self.supported_protocols = [Protocols.Threads, Protocols.GameLoop]
                
    def visit_ClassDiagram(self, class_diagram):
        self.fOut.write("/*")
        self.fOut.indent()
        self.fOut.write("Statecharts + Class Diagram compiler by Glenn De Jonghe")
        self.fOut.write()
        self.fOut.write("Source: " + class_diagram.source)
        self.fOut.write("Date:   " + time.asctime())
        if class_diagram.name or class_diagram.author or class_diagram.description:
            self.fOut.write()
        if class_diagram.author:
            self.fOut.write("Model author: " + class_diagram.author)
        if class_diagram.name:
            self.fOut.write("Model name:   " + class_diagram.name)
        if class_diagram.description.strip():
            self.fOut.write("Model description:")
            self.fOut.write()
            self.fOut.indent()
            self.fOut.write(class_diagram.description.strip())
            self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.write('*/')
        self.fOut.write()
        
        #Use runtime libraries
        self.fOut.write('using System;')
        self.fOut.write('using System.Collections.Generic;')
        self.fOut.write('using sccdlib;')

        #User imports
        if class_diagram.top.strip():
            StringUtils.writeCodeCorrectIndent(class_diagram.top, self.fOut)
        self.fOut.write()
        
        #visit children
        for c in class_diagram.classes :
            c.accept(self)
         
        #writing out ObjectManager
        self.fOut.write('public class ObjectManager : ObjectManagerBase')
        self.fOut.write('{')
        self.fOut.indent()
        self.fOut.write('public ObjectManager(ControllerBase controller): base(controller)')
        self.fOut.write("{")
        self.fOut.write("}")
        self.fOut.write()
        
        self.fOut.write('protected override InstanceWrapper instantiate(string class_name, object[] construct_params)')
        self.fOut.write('{')
        self.fOut.indent()
        self.fOut.write("RuntimeClassBase instance = null;")
        self.fOut.write("List<Association> associations = new List<Association>();")
        for index, c in enumerate(class_diagram.classes) :
            if index == 0 :
                self.fOut.write()
            else :
                self.fOut.write('}else ')
            self.fOut.extendWrite('if (class_name == "' + c.name + '" ){')
            self.fOut.indent()
            self.fOut.write('object[] new_parameters = new object[construct_params.Length + 1];')
            self.fOut.write('new_parameters[0] = this.controller;')
            self.fOut.write('Array.Copy(construct_params, 0, new_parameters, 1, construct_params.Length);')
            self.fOut.write('instance = (RuntimeClassBase) Activator.CreateInstance(typeof(' + c.name + '), new_parameters);')
            for a in c.associations :
                a.accept(self)
            self.fOut.dedent()
            if index == len(class_diagram.classes)-1 :
                self.fOut.write('}')
            
        self.fOut.write('if (instance != null) {')
        self.fOut.indent()
        self.fOut.write('return new InstanceWrapper(instance, associations);')
        self.fOut.dedent()
        self.fOut.write('}')
        self.fOut.write('return null;')
        self.fOut.dedent()
        self.fOut.write('}')
        self.fOut.dedent()
        self.fOut.write('}')
        
        # write out controller
        self.fOut.write()
        if self.protocol == Protocols.Threads :
            controller_sub_class = "ThreadsControllerBase"
        elif self.protocol == Protocols.GameLoop :
            controller_sub_class = "GameLoopControllerBase"
        self.fOut.write("public class Controller : " + controller_sub_class)
        self.fOut.write("{")
        self.fOut.indent()
    
        # write out constructor(s)
        if class_diagram.default_class.constructors :
            for constructor in class_diagram.default_class.constructors :
                self.writeControllerConstructor(class_diagram, constructor.parameters)
        else :
            self.writeControllerConstructor(class_diagram)
        
        self.fOut.write("public static void Main()")
        self.fOut.write("{")
        self.fOut.indent()
        self.fOut.write("Controller controller = new Controller();")
        self.fOut.write("controller.start();")
        self.fOut.dedent()
        self.fOut.write("}")
        
        self.fOut.dedent()
        self.fOut.write("}")
        
    #helper method
    def writeControllerConstructor(self, class_diagram, parameters = []):
        self.fOut.write('public Controller(')
        self.writeFormalParameters(parameters + [FormalParameter("keep_running", "bool", "true")])
        self.fOut.extendWrite(") : base(keep_running)")
        self.fOut.write('{')
        self.fOut.indent()
        
        for p in class_diagram.inports:
            self.fOut.write('this.addInputPort("' + p + '");')
        for p in class_diagram.outports:
            self.fOut.write('this.addOutputPort("' + p + '");')
        self.fOut.write('this.object_manager = new ObjectManager(this);')
        actual_parameters = [p.getIdent() for p in parameters]
        self.fOut.write('this.object_manager.createInstance("'+ class_diagram.default_class.name +'", new object[]{' +  ', '.join(actual_parameters)+ '});')
        self.fOut.dedent()
        self.fOut.write('}')

    def visit_Class(self, class_node):
        """
        Generate code for Class construct
        """
        self.fOut.write()
        self.fOut.write("public class " + class_node.name )
        # Take care of inheritance
        if len(class_node.super_classes) > 1 :
            raise Exception("C# doesn't allow multiple inheritance.");
        elif len(class_node.super_classes) == 1 :
            self.fOut.extendWrite(" : " + class_node.super_classes[0])
        else :
            self.fOut.extendWrite(" : " + "RuntimeClassBase")
        self.fOut.write("{")
        self.fOut.indent()
        self.fOut.write()
        
        if class_node.statechart is not None:
            # assign each node a unique ID
            self.fOut.write("/// <summary>")
            self.fOut.write("/// Enum uniquely representing all statechart nodes.")
            self.fOut.write("/// </summary>")
            self.fOut.write("public enum Node {")
            self.fOut.indent()
            for node in class_node.statechart.composites + class_node.statechart.basics:
                self.fOut.write(node.getFullName() + ",");
            self.fOut.dedent();
            self.fOut.write("};")
            self.fOut.write()
            self.fOut.write("Dictionary<Node,List<Node>> current_state = new Dictionary<Node,List<Node>>();");
            if len(class_node.statechart.historys) > 0 :
                self.fOut.write("Dictionary<Node,List<Node>> history_state = new Dictionary<Node,List<Node>>();");
            if class_node.statechart.nr_of_after_transitions != 0:
                self.fOut.write("Dictionary<int,double> timers = new Dictionary<int,double>();")
            self.fOut.write();
            
        #User defined attributes
        if class_node.attributes:
            self.fOut.write("//User defined attributes")
            for attribute in class_node.attributes:
                self.fOut.write(attribute.type + " " + attribute.name)
                if attribute.init_value is not None :
                    self.fOut.write(" = " + attribute.init_value);
                self.fOut.extendWrite(";")     
            self.fOut.write()

        if class_node.statechart is not None:  
            self.fOut.write("/// <summary>")
            self.fOut.write("/// Constructor part that is common for all constructors.")
            self.fOut.write("/// </summary>")
            self.fOut.write("private void commonConstructor(ControllerBase controller = null)")
            self.fOut.write("{")
            self.fOut.indent() 
            self.fOut.write("this.controller = controller;")
            self.fOut.write("this.object_manager = controller.getObjectManager();")

            self.fOut.write()
            self.fOut.write("//Initialize statechart")

            if class_node.statechart.historys:
                for node in class_node.statechart.historyParents:
                    self.fOut.write("this.history_state[Node." + node.getFullName() + "] = new List<Node>();")
                self.fOut.write()

            for node in class_node.statechart.composites :
                self.fOut.write("this.current_state[Node." + node.getFullName() + "] = new List<Node>();")
                
        self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.write()
        
        self.fOut.write("public override void start()")
        self.fOut.write("{")
        
        self.fOut.indent()
        self.fOut.write("base.start();")
        for default_node in class_node.statechart.root.defaults:
            if default_node.isComposite():
                self.fOut.write("this.enterDefault_" + default_node.getFullName() + "();")
            elif default_node.isBasic():
                self.fOut.write("this.enter_" + default_node.getFullName() + "();")
        self.fOut.dedent()
        self.fOut.write("}")
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
        self.fOut.write("}")
        self.fOut.write()

    def writeFormalParameters(self, parameters = []):
        """Helper method that writes a correct comma separated list of formal parameters"""    
        first = True       
        for param in parameters :
            if first :
                first = False
            else :
                self.fOut.extendWrite(', ')
            param.accept(self)
        
    def visit_FormalParameter(self, formal_parameter):
        self.fOut.extendWrite(formal_parameter.getType() + " " + formal_parameter.getIdent())
        if formal_parameter.hasDefault() :
            self.fOut.extendWrite(" = " + formal_parameter.getDefault())
                    
    def visit_Constructor(self, constructor):

        self.fOut.write(constructor.access + " " + constructor.parent_class.getName() + "(")
        self.writeFormalParameters([FormalParameter("controller", "ControllerBase", None)] + constructor.getParams())
        self.fOut.extendWrite(")")
        self.fOut.write("{")
        self.fOut.indent()
        self.fOut.write("this.commonConstructor(controller);")
        if constructor.body :
            self.fOut.write()
            self.fOut.write("//constructor body (user-defined)")
            StringUtils.writeCodeCorrectIndent(constructor.body, self.fOut)
        self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.write()
        
    def visit_Destructor(self, destructor):
        self.fOut.write("~" + destructor.parent_class.getName() + "()")
        self.fOut.write("{")
        if destructor.body :
            self.fOut.indent()
            StringUtils.writeCodeCorrectIndent(destructor.body, self.fOut)
            self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.write()
        
    def visit_Method(self, method):
        self.fOut.write(method.access + " " + method.return_type + "_" + method.parent_class.getName() + "(")
        self.writeFormalParameters(method.getParams())
        self.fOut.extendWrite(")\n{")
        self.fOut.indent()
        if method.body :
            self.fOut.indent()
            StringUtils.writeCodeCorrectIndent(method.body, self.fOut)
            self.fOut.dedent()
        self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.write()
        
    def visit_Association(self, association):
        self.fOut.write('associations.Add(new Association("' + association.name + '", "' + association.to_class + '", ' + str(association.min) + ', ' + str(association.max) + '));')
        
    #helper method
    def writeTransitionsRecursively(self, current_node):
        self.fOut.write("private bool transition_" + current_node.getFullName() + "(Event e)")
        self.fOut.write("{")
        self.fOut.indent()
        
        valid_children = []
        for child in current_node.children :
            if child.isComposite() or child.isBasic() :
                valid_children.append(child)  
         
        self.fOut.write("bool catched = false;")
        do_dedent = False
        if current_node.solvesConflictsOuter() :
            self.writeFromTransitions(current_node)
            if current_node.isParallel() or current_node.isComposite() :
                self.fOut.write("if (!catched){")
                self.fOut.indent()
                do_dedent = True
            
        if current_node.isParallel():
            for child in valid_children :     
                self.fOut.write("catched = this.transition_" + child.getFullName() + "(e) || catched;")
        elif current_node.isComposite():
            self.fOut.write()
            for i, child in enumerate(valid_children) :
                if i > 0 :
                    self.fOut.extendWrite(" else ")
                self.fOut.extendWrite("if (this.current_state[Node." + current_node.getFullName() + "][0] == Node." + child.getFullName() + "){")
                self.fOut.indent()
                self.fOut.write("catched = this.transition_" + child.getFullName() + "(e);")
                self.fOut.dedent()
                self.fOut.write("}")
                
        if current_node.solvesConflictsOuter() :
            if do_dedent :
                self.fOut.dedent()
                self.fOut.write("}")
        elif len(current_node.getTransitions()) > 0 :
                self.fOut.write("if (!catched) {")
                self.fOut.indent()
                self.writeFromTransitions(current_node)
                self.fOut.dedent()
                self.fOut.write("}")
            
        self.fOut.write("return catched;")
        self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.write();
        
        for child in valid_children :
            self.writeTransitionsRecursively(child)
                
    #helper method
    def writeFromTransitions(self, current_node): 
        # get all transition out of this state
        out_transitions = current_node.getTransitions()
        if len(out_transitions) == 0 :
            return
        
        self.fOut.write('var enableds = new List<int>();')
        for index, transition in enumerate(out_transitions, start=1):
            self.writeTransitionCondition(transition, index)
            
        self.fOut.write("if (enableds.Count > 1){")
        self.fOut.indent()
        self.fOut.write('Console.WriteLine("Runtime warning : indeterminism detected in a transition from node ' +  current_node.getFullID()+ '. Only the first in document order enabled transition is executed.");')
        self.fOut.dedent()
        self.fOut.write('}')
        self.fOut.write("if (enableds.Count > 0){")
        self.fOut.indent()
        self.fOut.write('int enabled = enableds[0];')
        self.fOut.write()      
              
        for index, transition in enumerate(out_transitions, start=1):
            self.writeTransitionAction(transition, index)
        
        self.fOut.write('catched = true;')   
        self.fOut.dedent()
        self.fOut.write('}')         
        self.fOut.write()
        
    def visit_FormalEventParameter(self, formal_event_parameter):
        self.fOut.extendWrite(formal_event_parameter.getType() + " " + formal_event_parameter.getName())
        
    def writeFormalEventParameters(self, transition):
        parameters = transition.getTrigger().getParameters()
        if(len(parameters) > 0) :
            self.fOut.write('object[] parameters = e.getParameters();')
            for index, parameter in enumerate(parameters):
                self.fOut.write()
                parameter.accept(self)
                self.fOut.extendWrite(' = (' + parameter.getType() + ')parameters[' + str(index) + '];')
        
    def writeTransitionAction(self, transition, index):
        if index > 1 :
            self.fOut.extendWrite(" else ")
        else :
            self.fOut.write()
        self.fOut.extendWrite("if (enabled == " + str(index) + "){")
        self.fOut.indent()

        # handle parameters to actually use them             
        self.writeFormalEventParameters(transition)
        
        exits = transition.getExitNodes()
        
        # write out exit actions
        if not exits[-1].isBasic():
            self.fOut.write("this.exit_" + exits[-1].getFullName() + "();")
        else:
            for node in exits:
                if node.isBasic():
                    self.fOut.write("this.exit_" + node.getFullName() + "();")
                    
        # write out trigger actions
        transition.getAction().accept(self)
        
   
        for (entering_node, is_ending_node) in transition.getEnterNodes() : 
            if is_ending_node :
                if entering_node.isComposite():
                    self.fOut.write("this.enterDefault_" + entering_node.getFullName() + "();")
                elif entering_node.isHistory():
                    self.fOut.write("this.enterHistory_" + entering_node.getParentNode().getFullName() + "(" + ("true" if entering_node.isHistoryDeep() else "false") + ");")
                else:
                    self.fOut.write("this.enter_" + entering_node.getFullName() + "();")
            else :
                if entering_node.isComposite():
                    self.fOut.write("this.enter_" + entering_node.getFullName() + "();")


        self.fOut.dedent()
        self.fOut.write('}')
                        
    def writeTransitionCondition(self, transition, index):
        trigger = transition.getTrigger()
        if not trigger.isUC():  
            self.fOut.write('if (e.getName() == "' + trigger.getEvent() + '" && e.getPort() == "' + trigger.getPort() + '"){')
            self.fOut.indent()   
        # evaluate guard
        if transition.hasGuard() :   
            # handle parameters for guard evaluation       
            self.writeFormalEventParameters(transition)  

            self.fOut.write('if (')
            transition.getGuard().accept(self)
            self.fOut.extendWrite('){')
            self.fOut.indent()    
            
        self.fOut.write("enableds.Add(" + str(index) + ");")

        if transition.hasGuard() :
            self.fOut.dedent()
            self.fOut.write('}')
        if not trigger.isUC() :
            self.fOut.dedent()
            self.fOut.write('}')
        self.fOut.write()
    
    def visit_EnterAction(self, enter_method):
        parent_node = enter_method.parent_node
        self.fOut.write("private void enter_" + parent_node.getFullName() + "()")
        self.fOut.write("{")
        self.fOut.indent()
        
        # take care of any AFTER events
        for transition in parent_node.transitions :
            trigger = transition.getTrigger()
            if trigger.isAfter() :
                self.fOut.write("this.timers[" + str(trigger.getAfterIndex()) + "] = ")
                trigger.after.accept(self)
                self.fOut.extendWrite(";")
        if enter_method.action:
            enter_method.action.accept(self)
        self.fOut.write("this.current_state[Node." + parent_node.getParentNode().getFullName() + "].Add(Node." + parent_node.getFullName() + ");")
        self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.write()
        
    #helper method
    def writeEnterDefault(self, entered_node):
        self.fOut.write("private void enterDefault_" + entered_node.getFullName() + "()")
        self.fOut.write("{")
        self.fOut.indent()
        self.fOut.write("this.enter_" + entered_node.getFullName() + "();")
        if entered_node.isComposite():
            l = entered_node.getDefaults()
            for i in l:
                if i.isComposite():
                    self.fOut.write("this.enterDefault_" + i.getFullName() + "();")
                elif i.isBasic():
                    self.fOut.write("this.enter_" + i.getFullName() + "();")
        self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.write()
         
    def visit_ExitAction(self, exit_method):
        exited_node = exit_method.parent_node
        self.fOut.write("private void exit_" + exited_node.getFullName() + "()")
        self.fOut.write("{")
        self.fOut.indent()
        #If the exited node is composite take care of potential history and the leaving of descendants
        if exited_node.isComposite() :
            #handle history
            if exited_node in exited_node.parent_statechart.historyParents:
                self.fOut.write("this.history_state[Node." + exited_node.getFullName() + "].AddRange(this.current_state[Node." + exited_node.getFullName() + "]);")
            
            #Take care of leaving children
            children = exited_node.getChildren()
            if exited_node.isParallel():
                for child in children:
                    if not child.isHistory() :
                        self.fOut.write("this.exit_" + child.getFullName() + "();")
            else:
                for child in children:
                    if not child.isHistory() :
                        self.fOut.write("if (this.current_state[Node." + exited_node.getFullName() + "].Contains(Node." + child.getFullName() +  ")){")
                        self.fOut.indent()
                        self.fOut.write("this.exit_" + child.getFullName() + "();")
                        self.fOut.dedent()  
                        self.fOut.write("}")
        
        
        # take care of any AFTER events
        for transition in exited_node.transitions :
            trigger = transition.getTrigger()
            if trigger.isAfter() :
                self.fOut.write("this.timers.Remove(" + str(trigger.getAfterIndex()) + ");")
                
        #Execute user-defined exit action if present
        if exit_method.action:
            exit_method.action.accept(self)
            
        #Adjust state
        self.fOut.write("this.current_state[Node." + exited_node.getParentNode().getFullName() + "].Remove(Node." + exited_node.getFullName() + ");")

        self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.write()
        
            
    #helper method
    def writeEnterHistory(self, entered_node):
        self.fOut.write("private void enterHistory_" + entered_node.getFullName() + "(")
        self.writeFormalParameters([FormalParameter("deep","bool")])
        self.fOut.extendWrite(")")
        self.fOut.write("{")
        self.fOut.indent()
        self.fOut.write("if (this.history_state[Node." + entered_node.getFullName() + "].Count == 0){")
        self.fOut.indent()
        defaults = entered_node.getDefaults()

        for node in defaults:
            if node.isBasic() :
                self.fOut.write("this.enter_" + node.getFullName() + "();")
            elif node.isComposite() :
                self.fOut.write("this.enterDefault_" + node.getFullName() + "();")

        self.fOut.dedent()
        self.fOut.write("} else {")
        self.fOut.indent()
        children = entered_node.getChildren()
        if entered_node.isParallel():
            for child in children:
                if not child.isHistory() :
                    self.fOut.write("this.enterHistory_" + child.getFullName() + "(deep);")
        else:
            for child in children:
                if not child.isHistory() :
                    self.fOut.write("if (this.history_state[Node." + entered_node.getFullName() + "].Contains(Node." + child.getFullName() + ")){")
                    self.fOut.indent()
                    if child.isComposite():
                        self.fOut.write("if (deep){")
                        self.fOut.indent()
                        self.fOut.write("this.enter_" + child.getFullName() + "();")
                        self.fOut.write("this.enterHistory_" + child.getFullName() + "(deep);")
                        self.fOut.dedent()
                        self.fOut.write("} else {")
                        self.fOut.indent()
                        self.fOut.write("this.enterDefault_" + child.getFullName() + "();")
                        self.fOut.dedent()
                        self.fOut.write("}")
                    else:
                        self.fOut.write("this.enter_" + child.getFullName() + "();")
                    self.fOut.dedent()
                    self.fOut.write("}")
        self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.write()

    def visit_StateChart(self, statechart):
        self.fOut.write("//Statechart enter/exit action method(s) :")
        self.fOut.write()
        
        #visit children
        for i in statechart.composites + statechart.basics:
            if i is not statechart.root :
                i.getEnterAction().accept(self)
                i.getExitAction().accept(self)

        # write out statecharts methods for enter/exit state
        if len(statechart.composites) > 1 :
            self.fOut.write("//Statechart enter/exit default method(s) :")
            self.fOut.write()
            for i in statechart.composites :
                if i is not statechart.root :
                    self.writeEnterDefault(i)

        # write out statecharts methods for enter/exit history
        if statechart.historys:
            self.fOut.write("//Statechart enter/exit history method(s) :")
            self.fOut.write()
            for i in statechart.historyParents:
                self.writeEnterHistory(i)     
                

        self.writeTransitionsRecursively(statechart.root)            
                
        # write out transition function
        self.fOut.write("protected override void transition (Event e = null)")
        self.fOut.write("{")
        self.fOut.indent()
        self.fOut.write("if (e == null) {");
        self.fOut.indent()
        self.fOut.write("e = new Event();")
        self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.write("this.state_changed = this.transition_" + statechart.root.getFullName() + "(e);")
        self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.write()

        # write out inState function
        self.fOut.write("public bool inState(List<Node> nodes)")
        self.fOut.write("{")
        self.fOut.indent()
        self.fOut.write("foreach(List<Node> actives in current_state.Values){")
        self.fOut.indent()
        self.fOut.write("foreach(Node node in actives)")
        self.fOut.indent()
        self.fOut.write("nodes.Remove (node);")
        self.fOut.dedent()
        self.fOut.write("if (nodes.Count == 0){")
        self.fOut.indent()
        self.fOut.write("return true;")
        self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.write("return false;")
        self.fOut.dedent()
        self.fOut.write("}")
        self.fOut.write()
        
    def visit_ExpressionPartString(self, bare_string):
        self.fOut.extendWrite(bare_string.string)
        
    def visit_SelfReference(self, self_reference):
        self.fOut.extendWrite("this")
        
    def visit_StateReference(self, state_ref):
        self.fOut.extendWrite("new List<Node>() {")
        self.fOut.extendWrite(", ".join(["Node." + node.getFullName() for node in state_ref.getNodes()]))
        self.fOut.extendWrite("}")
        
    def visit_InStateCall(self, in_state_call):
        self.fOut.extendWrite("this.inState(")
        in_state_call.target.accept(self)
        self.fOut.extendWrite(")")
        
    def visit_RaiseEvent(self, raise_event):
        if raise_event.isNarrow() or raise_event.isBroad():
            self.fOut.write('Event send_event = new Event("' + raise_event.getEventName() + '", "", new object[] {')
        elif raise_event.isLocal():
            self.fOut.write('this.addEvent( new Event("' + raise_event.getEventName() +'", "", new object[] {')
        elif raise_event.isOutput():
            self.fOut.write('this.controller.outputEvent(new Event("' + raise_event.getEventName() + '", "' + raise_event.getPort() + '", new object[] {')
        elif raise_event.isCD():
            self.fOut.write('this.object_manager.addEvent(new Event("' + raise_event.getEventName() + '", "", new object[] { this, ')
        first_param = True
        for param in raise_event.getParameters() :
            if first_param :
                first_param = False
            else :
                self.fOut.extendWrite(',')
            param.accept(self)
        if raise_event.isNarrow():
            self.fOut.extendWrite('});')
            self.fOut.write('this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, "' + raise_event.getTarget() + '" ,send_event}));')
        elif raise_event.isBroad():
            self.fOut.extendWrite('});')
            self.fOut.write('this.object_manager.addEvent(new Event("broad_cast", "", new object[] {send_event}));')
        else :
            self.fOut.extendWrite('}));')
            
    def visit_Script(self, script):
        StringUtils.writeCodeCorrectIndent(script.code, self.fOut)
        
    def visit_Log(self, log):
        self.fOut.write('Console.WriteLine("' + log.message + '");')
        
    def visit_Assign(self, assign):
        self.fOut.write()
        assign.lvalue.accept(self)
        self.fOut.extendWrite(" = ")
        assign.expression.accept(self)
        self.fOut.extendWrite(";")
        
