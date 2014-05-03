using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class StateChartTransition : Visitable
    {
        public TriggerEvent trigger { get; private set; }

        public StateChartNode parent { get; private set; }

        public Expression guard { get; private set; }

        public StateReference target { get; private set; }

        public Action action { get; private set; }

        /// <summary>
        /// Ordered list of nodes to be entered upon taking the transition. 
        /// The boolean value indicates whether the <c>StateChartNode</c> in the list is the last in a branch. 
        /// (And was thus specified specifically in the state reference.)
        /// </summary>
        /// <value>Set by the Path Calculator visitor.</value>
        public List<Tuple<StateChartNode,bool>> enter_nodes { get; set; }

        /// <summary>
        /// Ordered list of nodes to be exited upon taking the transition.
        /// </summary>
        /// <value>Set by the Path Calculator visitor.</value>
        public List<StateChartNode> exit_nodes { get; set; }

        public StateChartTransition(XElement xml, StateChartNode parent)
        {
            this.parent = parent;
            this.trigger = new TriggerEvent(xml);
            XAttribute guard_attribute = xml.Attribute("cond");
            if (guard_attribute != null && guard_attribute.Value.Trim() != "")
                this.guard = new Expression(guard_attribute.Value.Trim());
            else
                this.guard = null;

            XAttribute target_attribute = xml.Attribute("target");
            if (target_attribute == null)
                throw new CompilerException(string.Format("Transition from <{0}> is missing a target attribute.", this.parent.full_name));
            this.target = new StateReference(target_attribute.Value.Trim());

            this.action = new Action(xml);
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

