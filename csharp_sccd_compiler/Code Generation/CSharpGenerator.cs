using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace csharp_sccd_compiler
{
    public class CSharpGenerator : CodeGenerator
    {

        public CSharpGenerator() : base( new Platform[]{Platform.THREADS, Platform.GAMELOOP} )
        {
        }

        public override void visit(ClassDiagram class_diagram)
        {
            this.output_file.write("/*");
            this.output_file.indent();
            this.output_file.write("Statecharts + Class Diagram compiler by Glenn De Jonghe");
            this.output_file.write();
            this.output_file.write(string.Format("Generated on {0}.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            if  (class_diagram.model_name != null || class_diagram.model_author != null || class_diagram.model_description != null)
                this.output_file.write();

            if (class_diagram.model_name != null)
                this.output_file.write("Model name:   " + class_diagram.model_name);
            if (class_diagram.model_author != null)
                this.output_file.write("Model author: " + class_diagram.model_author);

            if (class_diagram.model_description != null)
            {
                this.output_file.write("Model description:");
                this.output_file.write();
                this.output_file.indent();
                this.writeCorrectIndent(class_diagram.model_description);
                this.output_file.dedent();
            }
            this.output_file.dedent();
            this.output_file.write("*/");
            this.output_file.write();
            
            //Use runtime libraries
            this.output_file.write("using System;");
            this.output_file.write("using System.Collections.Generic;");
            this.output_file.write("using sccdlib;");

            //Namespace using declarations by the user
            if (class_diagram.top_section != null)
                this.writeCorrectIndent(class_diagram.top_section);
            this.output_file.write();
            
            //visit children
            foreach (Class c in class_diagram.classes)
                c.accept(this);
             
            //writing out ObjectManager
            this.output_file.write("public class ObjectManager : ObjectManagerBase");
            this.output_file.write("{");
            this.output_file.indent();
            this.output_file.write("public ObjectManager(ControllerBase controller): base(controller)");
            this.output_file.write("{");
            this.output_file.write("}");
            this.output_file.write();
            
            this.output_file.write("protected override InstanceWrapper instantiate(string class_name, object[] construct_params)");
            this.output_file.write("{");
            this.output_file.indent();
            this.output_file.write("RuntimeClassBase instance = null;");
            this.output_file.write("List<Association> associations = new List<Association>();");
            for (int index = 0; index < class_diagram.classes.Count; ++index)
            {
                if (index == 0)
                    this.output_file.write();
                else
                    this.output_file.write("}else ");
                this.output_file.extendWrite("if (class_name == \"" + class_diagram.classes[index].name + "\" ){");
                this.output_file.indent();
                this.output_file.write("object[] new_parameters = new object[construct_params.Length + 1];");
                this.output_file.write("new_parameters[0] = this.controller;");
                this.output_file.write("Array.Copy(construct_params, 0, new_parameters, 1, construct_params.Length);");
                this.output_file.write("instance = (RuntimeClassBase) Activator.CreateInstance(typeof(" + class_diagram.classes[index].name + "), new_parameters);");
                foreach (Association association in class_diagram.classes[index].associations)
                    association.accept(this);
                this.output_file.dedent();
                if (index == class_diagram.classes.Count - 1)
                    this.output_file.write("}");
            }
            this.output_file.write("if (instance != null) {");
            this.output_file.indent();
            this.output_file.write("return new InstanceWrapper(instance, associations);");
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write("return null;");
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.dedent();
            this.output_file.write("}");

            //Write out controller
            this.output_file.write();
            string controller_sub_class = "";
            if (this.current_platform == Platform.THREADS)
                controller_sub_class = "ThreadsControllerBase";
            else if (this.current_platform == Platform.GAMELOOP)
                controller_sub_class = "GameLoopControllerBase";
            this.output_file.write("public class Controller : " + controller_sub_class);
            this.output_file.write("{");
            this.output_file.indent();
        
            //Write out constructor(s)
            if (class_diagram.default_class.constructors != null)
                foreach (Constructor constructor in class_diagram.default_class.constructors)
                    this.writeControllerConstructor(class_diagram, constructor.parameters);
            else
                this.writeControllerConstructor(class_diagram, new List<FormalParameter>());
            
            this.output_file.write("public static void Main()");
            this.output_file.write("{");
            this.output_file.indent();
            this.output_file.write("Controller controller = new Controller();");
            this.output_file.write("controller.start();");
            this.output_file.dedent();
            this.output_file.write("}");
            
            this.output_file.dedent();
            this.output_file.write("}");
        }

        /// <summary>
        /// Helper method
        /// </summary>
        private void writeControllerConstructor(ClassDiagram class_diagram, List<FormalParameter> parameters)
        {
            this.output_file.write("public Controller(");
            this.writeFormalParameters(parameters.Concat(new FormalParameter[]{new FormalParameter("keep_running", "bool", "true")}));
            this.output_file.extendWrite(") : base(keep_running)");
            this.output_file.write("{");
            this.output_file.indent();
            
            foreach (string p in class_diagram.inports)
                this.output_file.write("this.addInputPort(\"" + p + "\");");
            foreach (string p in class_diagram.outports)
                this.output_file.write("this.addOutputPort(\"" + p + "\");");
            this.output_file.write("this.object_manager = new ObjectManager(this);");
            string[] actual_parameters = (from parameter in parameters select parameter.name).ToArray();
            this.output_file.write("this.object_manager.createInstance(\"" + class_diagram.default_class.name + "\", new object[]{" + string.Join(", ", actual_parameters) + "});");
            this.output_file.dedent();
            this.output_file.write("}");
        }

        /// <summary>
        /// Generate code for Class construct
        /// </summary>
        public override void visit(Class class_node)
        {
            this.output_file.write();
            this.output_file.write("public class " + class_node.name);
            // Take care of inheritance
            if (class_node.super_class != null)
                this.output_file.extendWrite(" : " + class_node.super_class);
            else
                this.output_file.extendWrite(" : " + "RuntimeClassBase");
            this.output_file.write("{");
            this.output_file.indent();
            this.output_file.write();
            
            if (class_node.statechart != null)
            {
                //assign each node a unique ID
                this.output_file.write("/// <summary>");
                this.output_file.write("/// Enum uniquely representing all statechart nodes.");
                this.output_file.write("/// </summary>");
                this.output_file.write("public enum Node {");
                this.output_file.indent();
                foreach( StateChartNode node in class_node.statechart.composites.Concat(class_node.statechart.basics))
                    this.output_file.write(node.full_name + ",");
                this.output_file.dedent();
                this.output_file.write("};");
                this.output_file.write();
                this.output_file.write("Dictionary<Node,List<Node>> current_state = new Dictionary<Node,List<Node>>();");
                if (class_node.statechart.histories.Count > 0)
                    this.output_file.write("Dictionary<Node,List<Node>> history_state = new Dictionary<Node,List<Node>>();");
                this.output_file.write();
            }

            //User defined attributes
            if (class_node.attributes.Count > 0)
            {
                this.output_file.write("//User defined attributes");
                foreach (Attribute attribute in class_node.attributes)
                {
                    this.output_file.write(attribute.type + " " + attribute.name);
                    if (attribute.init_value != null)
                        this.output_file.extendWrite(" = " + attribute.init_value);
                    this.output_file.extendWrite(";");     
                }
                this.output_file.write();
            }

            if (class_node.statechart != null)
            {
                this.output_file.write("/// <summary>");
                this.output_file.write("/// Constructor part that is common for all constructors.");
                this.output_file.write("/// </summary>");
                this.output_file.write("private void commonConstructor(ControllerBase controller = null)");
                this.output_file.write("{");
                this.output_file.indent(); 
                this.output_file.write("this.controller = controller;");
                this.output_file.write("this.object_manager = controller.getObjectManager();");
                if (class_node.statechart.nr_of_after_transitions != 0)
                    this.output_file.write("this.timers = new Dictionary<int,double>();");

                this.output_file.write();
                this.output_file.write("//Initialize statechart :");
                this.output_file.write();

                if (class_node.statechart.histories.Count > 0)
                {
                    foreach (StateChartNode node in class_node.statechart.combined_history_parents)
                    {
                        this.output_file.write("this.history_state[Node." + node.full_name + "] = new List<Node>();");
                    }
                    this.output_file.write();
                }

                foreach (StateChartNode node in class_node.statechart.composites)
                    this.output_file.write("this.current_state[Node." + node.full_name + "] = new List<Node>();");
            }

            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write();
            
            this.output_file.write("public override void start()");
            this.output_file.write("{");
            
            this.output_file.indent();
            this.output_file.write("base.start();");
            foreach (StateChartNode default_node in class_node.statechart.root.defaults)
            {
                if (default_node.is_composite)
                    this.output_file.write("this.enterDefault_" + default_node.full_name + "();");
                else if (default_node.is_basic)
                    this.output_file.write("this.enter_" + default_node.full_name + "();");
            }

            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write();
            
            //visit children
            foreach( var i in class_node.constructors)
                i.accept(this);
            foreach( var i in class_node.destructors)
                i.accept(this);
            foreach( var i in class_node.methods)
                i.accept(this);
            if (class_node.statechart != null)
                class_node.statechart.accept(this);
              
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write();
        }

        /// <summary>
        /// Helper method that writes a correct comma separated list of formal parameters.
        /// </summary>
        private void writeFormalParameters(IEnumerable<FormalParameter> parameters)
        {
            bool first = true;       
            foreach (FormalParameter param in parameters)
            {
                if (first)
                    first = false;
                else
                    this.output_file.extendWrite(", ");
                param.accept(this);
            }
        }
            
        public override void visit(FormalParameter formal_parameter)
        {
            this.output_file.extendWrite(formal_parameter.type + " " + formal_parameter.name);
            if (formal_parameter.default_value != null)
                this.output_file.extendWrite(" = " + formal_parameter.default_value);
        }
                        
        public override void visit(Constructor constructor)
        {
            this.output_file.write(constructor.access + " " + constructor.name + "(");
            this.writeFormalParameters(new FormalParameter[]{new FormalParameter("controller", "ControllerBase", null)}.Concat(constructor.parameters));
            this.output_file.extendWrite(")");
            this.output_file.write("{");
            this.output_file.indent();
            this.output_file.write("this.commonConstructor(controller);");
            if (constructor.body != null && constructor.body.Trim() != "")
            {
                this.output_file.write();
                this.output_file.write("//constructor body (user-defined)");
                this.writeCorrectIndent(constructor.body);
            }
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write();
        }
            
        public override void visit(Destructor destructor)
        {
            this.output_file.write(destructor.name + "()");
            this.output_file.write("{");
            if (destructor.body != null)
            {
                this.output_file.indent();
                this.writeCorrectIndent(destructor.body);
                this.output_file.dedent();
            }
            this.output_file.write("}");
            this.output_file.write();
        }
            
        public override void visit(Method method)
        {
            this.output_file.write(method.access + " " + method.return_type + " " + method.name + "(");
            this.writeFormalParameters(method.parameters);
            this.output_file.extendWrite(")");
            this.output_file.write("{");
            this.output_file.indent();
            if (method.body != null)
            {
                this.output_file.indent();
                this.writeCorrectIndent(method.body);
                this.output_file.dedent();
            }
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write();
        }

        public override void visit(Association association)
        {
            this.output_file.write("associations.Add(new Association(\"" + association.name + "\", \"" + association.to_class + "\", " + association.min.ToString() + ", " + association.max.ToString() + "));");
        }

        /// <summary>
        /// Helper method that writes the transitions recursively.
        /// </summary>
        public void writeTransitionsRecursively(StateChartNode current_node)
        {
            this.output_file.write("private bool transition_" + current_node.full_name + "(Event e)");
            this.output_file.write("{");
            this.output_file.indent();
            
            List<StateChartNode> valid_children = new List<StateChartNode>();
            foreach (StateChartNode child in current_node.children)
            {
                if (child.is_composite || child.is_basic)
                    valid_children.Add(child);
            }
             
            this.output_file.write("bool catched = false;");
            bool do_dedent = false;
            if (current_node.solves_conflict_outer)
            {
                this.writeFromTransitions(current_node);
                if (current_node.is_parallel || current_node.is_composite)
                {
                    this.output_file.write("if (!catched){");
                    this.output_file.indent();
                    do_dedent = true;
                }
            }
                
            if (current_node.is_parallel)
            {
                foreach (StateChartNode child in valid_children) 
                    this.output_file.write("catched = this.transition_" + child.full_name + "(e) || catched;");
            }
            else if (current_node.is_composite)
            {
                this.output_file.write();
                for (int i=0; i < valid_children.Count; ++i)
                {
                    if (i > 0)
                        this.output_file.extendWrite(" else ");
                    this.output_file.extendWrite("if (this.current_state[Node." + current_node.full_name + "][0] == Node." + valid_children[i].full_name + "){");
                    this.output_file.indent();
                    this.output_file.write("catched = this.transition_" + valid_children[i].full_name + "(e);");
                    this.output_file.dedent();
                    this.output_file.write("}");
                }
            }   
            if (current_node.solves_conflict_outer)
            {
                if (do_dedent)
                {
                    this.output_file.dedent();
                    this.output_file.write("}");
                }
            }
            else if (current_node.transitions.Count > 0)
            {
                this.output_file.write("if (!catched) {");
                this.output_file.indent();
                this.writeFromTransitions(current_node);
                this.output_file.dedent();
                this.output_file.write("}");
            }
                
            this.output_file.write("return catched;");
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write();
            
            foreach (StateChartNode child in valid_children)
                this.writeTransitionsRecursively(child);
        }
                    
        /// <summary>
        /// Helper method
        /// </summary>
        private void writeFromTransitions(StateChartNode current_node)
        {
            if (current_node.transitions.Count == 0)
                return;
            
            this.output_file.write("List<int> enableds = new List<int>();");
            for (int index=0; index < current_node.transitions.Count; ++index)
                this.writeTransitionCondition(current_node.transitions[index], index);
                
            this.output_file.write("if (enableds.Count > 1){");
            this.output_file.indent();
            this.output_file.write("Console.WriteLine(\"Runtime warning : indeterminism detected in a transition from node " +  current_node.full_name + ". Only the first in document order enabled transition is executed.\");");
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write("if (enableds.Count > 0){");
            this.output_file.indent();
            this.output_file.write("int enabled = enableds[0];");
            this.output_file.write();
                  
            for (int index=0; index < current_node.transitions.Count; ++index)
                this.writeTransitionAction(current_node.transitions[index], index);
            
            this.output_file.write("catched = true;");   
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write();
        }
            
        public override void visit( FormalEventParameter formal_event_parameter)
        {
            this.output_file.extendWrite(formal_event_parameter.type + " " + formal_event_parameter.name);
        }
            
        /// <summary>
        /// Helper method
        /// </summary>
        private void writeFormalEventParameters(StateChartTransition transition)
        {
            if (transition.trigger.parameters.Count > 0)
            {
                this.output_file.write("object[] parameters = e.getParameters();");
                for (int index=0; index < transition.trigger.parameters.Count; ++index)
                {
                    this.output_file.write();
                    transition.trigger.parameters[index].accept(this);
                    this.output_file.extendWrite(" = (" + transition.trigger.parameters[index].type + ")parameters[" + index.ToString() + "];");
                }
            }
        }

        ///
        private void writeTransitionAction(StateChartTransition transition, int index)
        {
            if (index > 1)
                this.output_file.extendWrite(" else ");
            else
                this.output_file.write();
            this.output_file.extendWrite("if (enabled == " + index.ToString() + "){");
            this.output_file.indent();

            //Handle parameters to actually use them             
            this.writeFormalEventParameters(transition);
            
            //Write out exit actions
            StateChartNode last_exit_node = transition.exit_nodes[transition.exit_nodes.Count - 1];
            if (!last_exit_node.is_basic)
                this.output_file.write("this.exit_" + last_exit_node.full_name + "();");
            else
            {
                foreach (StateChartNode node in transition.exit_nodes)
                {
                    if (node.is_basic)
                        this.output_file.write("this.exit_" + node.full_name + "();");
                }
            }

            //Write out trigger actions
            transition.action.accept(this);
            
            foreach (Tuple<StateChartNode,bool> enter_node_tuple in transition.enter_nodes)
            {
                StateChartNode entering_node = enter_node_tuple.Item1;
                if (enter_node_tuple.Item2)
                {
                    if (entering_node.is_composite)
                        this.output_file.write("this.enterDefault_" + entering_node.full_name + "();");
                    else if (entering_node.is_history)
                    {
                        if (entering_node.is_history_deep)
                            this.output_file.write("this.enterHistoryDeep_" + entering_node.parent.full_name + "();");
                        else
                            this.output_file.write("this.enterHistoryShallow_" + entering_node.parent.full_name + "();");
                    }
                    else
                        this.output_file.write("this.enter_" + entering_node.full_name + "();");
                }
                else
                {
                    if (entering_node.is_composite)
                        this.output_file.write("this.enter_" + entering_node.full_name + "();");
                }
            }
            this.output_file.dedent();
            this.output_file.write("}");
        }
                            
        private void writeTransitionCondition(StateChartTransition transition, int index)
        {
            if (!transition.trigger.is_uc)
            {
                this.output_file.write("if (e.getName() == \"" + transition.trigger.event_name + "\" && e.getPort() == \"" + transition.trigger.port + "\"){");
                this.output_file.indent();   
            }
            //Evaluate guard
            if (transition.guard != null)
            {
                //Handle parameters for guard evaluation       
                this.writeFormalEventParameters(transition);  

                this.output_file.write("if (");
                transition.guard.accept(this);
                this.output_file.extendWrite("){");
                this.output_file.indent();    
            }
            this.output_file.write("enableds.Add(" + index.ToString() + ");");

            if (transition.guard != null)
            {
                this.output_file.dedent();
                this.output_file.write("}");
            }
            if (!transition.trigger.is_uc)
            {
                this.output_file.dedent();
                this.output_file.write("}");
            }
            this.output_file.write();
        }
        
        public override void visit(EnterAction enter_method)
        {
            this.output_file.write("private void enter_" + enter_method.parent.full_name + "()");
            this.output_file.write("{");
            this.output_file.indent();
            
            //Take care of any AFTER events
            foreach (StateChartTransition transition in enter_method.parent.transitions)
            {
                if (transition.trigger.is_after)
                {
                    this.output_file.write("this.timers[" + transition.trigger.after_index.ToString() + "] = ");
                    transition.trigger.after_expression.accept(this);
                    this.output_file.extendWrite(";");
                }
            }
            if (enter_method.action != null)
                enter_method.action.accept(this);
            this.output_file.write("this.current_state[Node." + enter_method.parent.parent.full_name + "].Add(Node." + enter_method.parent.full_name + ");");
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write();
        }

        private void writeEnterDefault(StateChartNode entered_node)
        {
            this.output_file.write("private void enterDefault_" + entered_node.full_name + "()");
            this.output_file.write("{");
            this.output_file.indent();
            this.output_file.write("this.enter_" + entered_node.full_name + "();");
            if (entered_node.is_composite)
            {
                foreach(StateChartNode default_node in entered_node.defaults)
                {
                    if (default_node.is_composite)
                        this.output_file.write("this.enterDefault_" + default_node.full_name + "();");
                    else if (default_node.is_basic)
                        this.output_file.write("this.enter_" + default_node.full_name + "();");
                }
            }
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write();
        }
             
        public override void visit(ExitAction exit_method)
        {
            this.output_file.write("private void exit_" + exit_method.parent.full_name + "()");
            this.output_file.write("{");
            this.output_file.indent();
            //If the exited node is composite take care of potential history and the leaving of descendants
            if (exit_method.parent.is_composite)
            {
                //handle history
                if (exit_method.parent.save_state_on_exit)
                    this.output_file.write("this.history_state[Node." + exit_method.parent.full_name + "].AddRange(this.current_state[Node." + exit_method.parent.full_name + "]);");
                
                //Take care of leaving children
                if (exit_method.parent.is_parallel)
                {
                    foreach(StateChartNode child in exit_method.parent.children)
                    {
                        if (!child.is_history)
                            this.output_file.write("this.exit_" + child.full_name + "();");
                    }
                }
                else
                {
                    foreach(StateChartNode child in exit_method.parent.children)
                    {
                        if (!child.is_history)
                        {
                            this.output_file.write("if (this.current_state[Node." + exit_method.parent.full_name + "].Contains(Node." + child.full_name + ")){");
                            this.output_file.indent();
                            this.output_file.write("this.exit_" + child.full_name + "();");
                            this.output_file.dedent();
                            this.output_file.write("}");
                        }
                    }
                }
            }

            //Take care of any AFTER events
            foreach (StateChartTransition transition in exit_method.parent.transitions)
            {
                if (transition.trigger.is_after)
                    this.output_file.write("this.timers.Remove(" + transition.trigger.after_index.ToString() + ");");
            } 

            //Execute user-defined exit action if present
            if (exit_method.action != null)
                exit_method.action.accept(this);
                
            //Adjust state
            this.output_file.write("this.current_state[Node." + exit_method.parent.parent.full_name + "].Remove(Node." + exit_method.parent.full_name + ");");

            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write();
        }
           
        private void writeEnterHistory(StateChartNode entered_node, bool is_deep)
        {
            this.output_file.write("private void enterHistory");
            if (is_deep)
                this.output_file.extendWrite("Deep");
            else
                this.output_file.extendWrite("Shallow");
            this.output_file.extendWrite("_" + entered_node.full_name + "()");
            this.output_file.write("{");
            this.output_file.indent();
            this.output_file.write("if (this.history_state[Node." + entered_node.full_name + "].Count == 0){");
            this.output_file.indent();

            foreach (StateChartNode node in entered_node.defaults)
            {
                if (node.is_basic)
                    this.output_file.write("this.enter_" + node.full_name + "();");
                else if (node.is_composite)
                    this.output_file.write("this.enterDefault_" + node.full_name + "();");
            }

            this.output_file.dedent();
            this.output_file.write("} else {");
            this.output_file.indent();

            if (entered_node.is_parallel)
            {
                foreach (StateChartNode child in entered_node.children)
                {
                    if (!child.is_history)
                    {
                        this.output_file.write("this.enterHistory");
                        if (is_deep)
                            this.output_file.extendWrite("Deep");
                        else
                            this.output_file.extendWrite("Shallow");
                        this.output_file.extendWrite("_" + child.full_name + "();");
                    }
                }
            }
            else
            {
                foreach (StateChartNode child in entered_node.children)
                {
                    if (!child.is_history)
                    {
                        this.output_file.write("if (this.history_state[Node." + entered_node.full_name + "].Contains(Node." + child.full_name + ")){");
                        this.output_file.indent();
                        if (child.is_composite)
                        {
                            if (is_deep)
                            {
                                this.output_file.write("this.enter_" + child.full_name + "();");
                                this.output_file.write("this.enterHistoryDeep_" + child.full_name + "();");
                            }
                            else
                                this.output_file.write("this.enterDefault_" + child.full_name + "();");
                        }
                        else
                            this.output_file.write("this.enter_" + child.full_name + "();");
                        this.output_file.dedent();
                        this.output_file.write("}");
                    }
                }
            }
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write();
        }

        public override void visit(StateChart statechart)
        {
            this.output_file.write("//Statechart enter/exit action method(s) :");
            this.output_file.write();
            
            //Visit enter and exit actions of children
            foreach (StateChartNode node in statechart.composites.Concat(statechart.basics))
            {
                if (!object.ReferenceEquals(node, statechart.root))
                {
                    node.enter_action.accept(this);
                    node.exit_action.accept(this);
                }
            }
            //Write out statecharts methods for enter/exit state
            if (statechart.composites.Count > 1)
            {
                this.output_file.write("//Statechart enter/exit default method(s) :");
                this.output_file.write();
                foreach (StateChartNode node in statechart.composites)
                {
                    if (!object.ReferenceEquals(node, statechart.root))
                        this.writeEnterDefault(node);
                }
            }

            //Write out statecharts methods for enter/exit history
            if (statechart.histories.Count > 0)
            {
                this.output_file.write("//Statechart enter/exit history method(s) :");
                this.output_file.write();
                foreach (StateChartNode node in statechart.shallow_history_parents)
                    this.writeEnterHistory(node, false);
                foreach (StateChartNode node in statechart.deep_history_parents)
                    this.writeEnterHistory(node, true);   
            }
            this.output_file.write("//Statechart transitions :");
            this.output_file.write();
            this.writeTransitionsRecursively(statechart.root);           
                    
            //Write out transition function
            this.output_file.write("protected override void transition (Event e = null)");
            this.output_file.write("{");
            this.output_file.indent();
            this.output_file.write("if (e == null) {");
            this.output_file.indent();
            this.output_file.write("e = new Event();");
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write("this.state_changed = this.transition_" + statechart.root.full_name + "(e);");
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write();

            //Write out inState function
            this.output_file.write("public bool inState(List<Node> nodes)");
            this.output_file.write("{");
            this.output_file.indent();
            this.output_file.write("foreach(List<Node> actives in current_state.Values){");
            this.output_file.indent();
            this.output_file.write("foreach(Node node in actives)");
            this.output_file.indent();
            this.output_file.write("nodes.Remove (node);");
            this.output_file.dedent();
            this.output_file.write("if (nodes.Count == 0){");
            this.output_file.indent();
            this.output_file.write("return true;");
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write("return false;");
            this.output_file.dedent();
            this.output_file.write("}");
            this.output_file.write();
        }

        public override void visit(ExpressionPartString expression_part_string)
        {
            this.output_file.extendWrite(expression_part_string.value);
        }
            
        public override void visit(SelfReference self_reference)
        {
            this.output_file.extendWrite("this");
        }
            
        public override void visit(StateReference state_ref)
        {
            this.output_file.extendWrite("new List<Node>() {");
            this.output_file.extendWrite(string.Join(", ", (from node in state_ref.target_nodes select "Node." + node.full_name)));
            this.output_file.extendWrite("}");
        }
            
        public override void visit(InStateCall in_state_call)
        {
            this.output_file.extendWrite("this.inState(");
            in_state_call.state_reference.accept(this);
            this.output_file.extendWrite(")");
        }
            
        public override void visit(RaiseEvent raise_event)
        {
            if (raise_event.scope == RaiseEvent.Scope.NARROW_SCOPE || raise_event.scope == RaiseEvent.Scope.BROAD_SCOPE)
                this.output_file.write("Event send_event = new Event(\"" + raise_event.event_name + "\", \"\", new object[] {");
            else if (raise_event.scope == RaiseEvent.Scope.LOCAL_SCOPE)
                this.output_file.write("this.addEvent( new Event(\"" + raise_event.event_name + "\", \"\", new object[] {");
            else if (raise_event.scope == RaiseEvent.Scope.OUTPUT_SCOPE)
                this.output_file.write("this.controller.outputEvent(new Event(\"" + raise_event.event_name + "\", \"" + raise_event.port + "\", new object[] {");
            else if (raise_event.scope == RaiseEvent.Scope.CD_SCOPE)
                this.output_file.write("this.object_manager.addEvent(new Event(\"" + raise_event.event_name + "\", \"\", new object[] { this, ");

            bool first_param = true;
            foreach (Expression param in raise_event.parameters)
            {
                if (first_param)
                    first_param = false;
                else
                    this.output_file.extendWrite(",");
                param.accept(this);
            }

            if (raise_event.scope == RaiseEvent.Scope.NARROW_SCOPE)
            {
                this.output_file.extendWrite("});");
                this.output_file.write("this.object_manager.addEvent(new Event(\"narrow_cast\", \"\", new object[] {this, \"" + raise_event.target + "\" ,send_event}));");
            }
            else if (raise_event.scope == RaiseEvent.Scope.BROAD_SCOPE)
            {
                this.output_file.extendWrite("});");
                this.output_file.write("this.object_manager.addEvent(new Event(\"broad_cast\", \"\", new object[] {send_event}));");
            }
            else
                this.output_file.extendWrite("}));");
        }
                
        public override void visit(Script script)
        {
            this.writeCorrectIndent(script.code);
        }
            
        public override void visit(Log log)
        {
            this.output_file.write("Console.WriteLine(\"" + log.message + "\");");
        }
            
        public override void visit(Assign assign)
        {
            this.output_file.write();
            assign.lvalue.accept(this);
            this.output_file.extendWrite(" = ");
            assign.expression.accept(this);
            this.output_file.extendWrite(";");
        }
    }
}

