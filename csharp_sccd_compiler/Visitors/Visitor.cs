using System;

namespace csharp_sccd_compiler
{
	abstract public class Visitor
	{
        public virtual void visit(Visitable visiting)
        {
        }

        public virtual void visit(ClassDiagram class_diagram)
        {
        }

        public virtual void visit(Class class_node)
        {
        }

        public virtual void visit(FormalParameter formal_parameter)
        {
        }

        public virtual void visit(Constructor constructor)
        {
        }

        public virtual void visit(Destructor destructor)
        {
        }

        public virtual void visit(Method method)
        {
        }

        public virtual void visit(Association association)
        {
        }

        public virtual void visit(EnterExitAction enter_exit_action)
        {
        }

        public virtual void visit(EnterAction enter_method)
        {
        }

        public virtual void visit(ExitAction exit_method)
        {
        }

        public virtual void visit(StateChart statechart)
        {
        }
        
        public virtual void visit(StateChartNode node)
        {
        }

        public virtual void visit(StateChartTransition transition)
        {
        }

        public virtual void visit(ExpressionPartString expression_part_string)
        {
        }

        public virtual void visit(SelfReference self_reference)
        {
        }

        public virtual void visit(StateReference state_ref)
        {
        }

        public virtual void visit(InStateCall in_state_call)
        {
        }

        public virtual void visit(RaiseEvent raise_event)
        {
        }

        public virtual void visit(Script script)
        {
        }

        public virtual void visit(Log log)
        {
        }

        public virtual void visit(Assign assign)
        {
        }

        public virtual void visit( FormalEventParameter formal_event_parameter)
        {
        }
	}
}

