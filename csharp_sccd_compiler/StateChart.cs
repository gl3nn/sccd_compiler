using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class StateChart : Visitable
    {
        /// <summary>
        /// The total number of transitions present in this statechart that are trigger by time (AFTER).
        /// </summary>
        public int nr_of_after_transitions { get; private set; }
        /// <summary>
        /// Root node of the statechart.
        /// </summary>
        public StateChartNode root { get; private set; } 
        /// <summary>
        /// All basic states.
        /// </summary>
        public List<StateChartNode> basics { get; private set; } 
        /// <summary>
        /// All composite states.
        /// </summary>
        public List<StateChartNode> composites { get; private set; }
        /// <summary>
        /// All history states.
        /// </summary>
        public List<StateChartNode> histories { get; private set; }
        /// <summary>
        /// All nodes that need their state saved shallow.
        /// </summary>
        public List<StateChartNode> shallow_history_parents { get; private set; }
        /// <summary>
        /// All nodes that need their state saved deep.
        /// </summary>
        public List<StateChartNode> deep_history_parents { get; private set; }
        /// <summary>
        /// All nodes that need their state saved on leaving
        /// </summary>
        public List<StateChartNode> combined_history_parents { get; private set; }

        public StateChart(XElement xml)
        {
            this.root = new StateChartNode(xml);

            this.nr_of_after_transitions = 0;
            this.basics = new List<StateChartNode>();
            this.composites = new List<StateChartNode>();
            this.histories = new List<StateChartNode>();

            this.extractFromHierarchy(this.root);

            this.shallow_history_parents = new List<StateChartNode>();
            this.deep_history_parents = new List<StateChartNode>();
            this.combined_history_parents = new List<StateChartNode>();

            foreach (StateChartNode node in this.histories)
            {
                this.calculateHistory(node.parent, node.is_history_deep);
            }
        }

        /// <summary>
        /// Calculates which nodes need their state saved for history purposes.
        /// </summary>
        /// <param name="parent">The parent node of a history state.</param>
        /// <param name="is_history_deep">If set to <c>true</c> the history type is <c>deep</c>.</param>
        private void calculateHistory(StateChartNode parent, bool is_history_deep)
        {
            if (object.ReferenceEquals(parent, this.root)) //TODO use is_root property?
                throw new CompilerException("Root component cannot contain a history state.");

            if (!this.combined_history_parents.Contains(parent))
            {
                this.combined_history_parents.Add(parent);
                parent.save_state_on_exit = true;
            }

            if (is_history_deep)
            {
                if (! this.deep_history_parents.Contains(parent))
                    this.deep_history_parents.Add(parent);
            }
            else
            {
                if (! this.shallow_history_parents.Contains(parent))
                    this.shallow_history_parents.Add(parent);
            }
            if ( parent.is_parallel || is_history_deep)
            {
                foreach (StateChartNode child in parent.children)
                    if (child.is_composite)
                        this.calculateHistory(child, is_history_deep);
            }
        }

        private void extractFromHierarchy(StateChartNode node)
        {
            foreach (StateChartTransition transition in node.transitions)
            {
                TriggerEvent trigger = transition.trigger;
                if (trigger.is_after)
                {
                    trigger.after_index = this.nr_of_after_transitions;
                    string event_name = string.Format("_{0}after", trigger.after_index);
                    trigger.event_name = event_name;
                    this.nr_of_after_transitions += 1;
                }
            }

            if (node.is_basic)
                this.basics.Add(node);
            else if (node.is_composite) 
                this.composites.Add(node);
            else if (node.is_history) 
                this.histories.Add(node);

            foreach (StateChartNode child in node.children)
                this.extractFromHierarchy(child);
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}