using System;
using System.Linq;
using System.Collections.Generic;

namespace csharp_sccd_compiler
{
    public class StateLinker : Visitor
    {
        StateChart visiting_statechart;
        StateChartNode visiting_node;
        Lexer lexer;

        public StateLinker()
        {
            this.lexer = new Lexer();
        }

        public override void visit(ClassDiagram class_diagram)
        {
            foreach (Class c in class_diagram.classes)
                c.accept(this);
        }

        public override void visit(Class c)
        {
            c.statechart.accept(this);
        }

        public override void visit(StateChart statechart)
        {
            this.visiting_statechart = statechart;
            foreach (StateChartNode node in statechart.basics.Concat(statechart.composites))
                node.accept(this);
        }
        
        public override void visit(StateChartNode node)
        {
            this.visiting_node = node;
            node.enter_action.accept(this);
            node.exit_action.accept(this);
            foreach (StateChartTransition transition in node.transitions)
                transition.accept(this);
        }
               

        public override void visit(StateChartTransition transition)
        {
            try
            {
                transition.target.accept(this);
            }
            catch (StateReferenceException exception)
            {
                throw new StateReferenceException(string.Format("Transition from <{0}> has invalid target.", this.visiting_node.full_name), exception);
            }
            try
            {
                transition.action.accept(this);
            }
            catch (StateReferenceException exception)
            {
                throw new StateReferenceException(string.Format("Transition from <{0}> has invalid action.", this.visiting_node.full_name), exception);
            }
            try
            {
                if (transition.guard != null)
                    transition.guard.accept(this);
            }
            catch (StateReferenceException exception)
            {
                throw new StateReferenceException(string.Format("Transition from <{0}> has invalid guard.", this.visiting_node.full_name), exception);
            }
        }

        public override void visit(StateReference state_reference)
        {
            state_reference.target_nodes = new List<StateChartNode>();
            StateChartNode current_node = null; //Will be used to find the target state(s)
            Stack<StateChartNode> split_stack = new Stack<StateChartNode>(); // user for branching
            this.lexer.setInput(state_reference.state_reference_string);

            foreach (Token token in this.lexer.iterateTokens())
            {
                if (current_node == null) //current_node is not set yet or has been reset, the CHILD token can now have a special meaning
                {
                    if (token.type == Token.Type.SLASH) 
                    {
                        current_node = this.visiting_statechart.root; //Root detected
                        continue;
                    }
                    else
                        current_node = this.visiting_node;
                }

                if (token.type == Token.Type.DOT)
                {
                    Token next_token = this.lexer.nextToken();//Advance to next token
                    if (next_token == null || next_token.type == Token.Type.SLASH)
                        continue;//CURRENT operator "." detected
                    else if (next_token.type == Token.Type.DOT)
                    {
                        next_token = this.lexer.nextToken();//Advance to next token
                        if (next_token == null || next_token.type == Token.Type.SLASH)
                        {
                            current_node = current_node.parent; //PARENT operator ".." detected
                            if (current_node == null)
                                throw new StateReferenceException(string.Format("Illegal use of PARENT \"..\" operator at position {0} in state reference. Root of statechart reached.", token.pos));
                        }
                        else
                            throw new StateReferenceException(string.Format("Illegal use of PARENT \"..\" operator at position {0} in state reference.", token.pos));
                    }
                    else
                        throw new StateReferenceException(string.Format("Illegal use of CURRENT \"..\" operator at position {0} in state reference.", token.pos));
                }
                else if (token.type == Token.Type.SLASH)
                    continue;
                else if (token.type == Token.Type.WORD)
                {
                    //Trying to advance to next child state
                    string child_name = token.val;
                    bool found = false;
                    foreach (StateChartNode child in current_node.children)
                    {
                        if (child.name == child_name)
                        {
                            found = true;
                            current_node = child;
                            break;
                        }
                    }
                    if (!found)
                        throw new StateReferenceException(string.Format("Refering to non exiting node at posisition {0} in state reference.", token.pos));
                }
                else if (token.type == Token.Type.LBRACKET)
                    split_stack.Push(current_node);
                else if (token.type == Token.Type.RBRACKET)
                {
                    if (split_stack.Count > 0)
                        split_stack.Pop();
                    else
                        throw new StateReferenceException(string.Format("Invalid token at position {0} in state reference.", token.pos));
                }
                else if (token.type == Token.Type.COMMA)
                {
                    state_reference.target_nodes.Add(current_node);
                    if (split_stack.Count > 0)
                        current_node = split_stack.Peek();
                    else
                        current_node = null;
                }
                else
                    throw new StateReferenceException(string.Format("Invalid token at position {0} in state reference.", token.pos));
            }

            if (split_stack.Count != 0 || current_node == null) //RB missing or extra COMMA
                throw new StateReferenceException("State reference ends unexpectedly.");

            //TODO better validation of the target! When is it a valid state configuration?
            foreach (StateChartNode node in state_reference.target_nodes)
            {
                if (object.ReferenceEquals(current_node,node))
                    throw new StateReferenceException("A state reference can't reference the same node more than once.");
                if (node.isDescendantOrAncestorOf(current_node))
                    throw new StateReferenceException("A state reference can't reference a node and one of its descendants.");
            }

            state_reference.target_nodes.Add(current_node);

            if (state_reference.target_nodes.Count == 0)
                throw new StateReferenceException("Meaningless state reference.");
        }

        public override void visit(EnterExitAction enter_exit_action)
        {
            if (enter_exit_action.action != null)
                enter_exit_action.action.accept(this);
        }

        public override void visit(InStateCall in_state_call)
        {
            try
            {
                in_state_call.state_reference.accept(this);
            }
            catch (StateReferenceException exception)
            {
                throw new StateReferenceException(string.Format("Invalid state reference for {0} macro.", Constants.INSTATE_SEQ), exception);
            }
        }

        public override void visit(Assign assign)
        {
            assign.expression.accept(this);
        }
    }
}

