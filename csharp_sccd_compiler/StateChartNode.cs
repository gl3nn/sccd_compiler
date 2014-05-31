using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class StateChartNode : Visitable
    {

        public bool is_basic { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="csharp_sccd_compiler.StateChartNode"/> is a parallel state.
        /// </summary>
        /// <value><c>true</c> if is_parallel; otherwise, <c>false</c>.</value>
        public bool is_parallel { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="csharp_sccd_compiler.StateChartNode"/> is a composite state.
        /// </summary>
        /// <value><c>true</c> if is_composite; otherwise, <c>false</c>.</value>
        public bool is_composite { get; private set; }

        public bool is_history { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="csharp_sccd_compiler.StateChartNode"/> is history state of type <c>deep</c>.
        /// </summary>
        /// <value><c>true</c> if is_history_deep; otherwise, <c>false</c>.</value>
        public bool is_history_deep { get; private set; }

        public bool is_root { get; private set; }

        public bool save_state_on_exit { get; set; }

        public bool solves_conflict_outer { get; private set; }

        public string name { get; private set; }

        public string full_name { get; private set; }

        /// <summary>
        /// The children nodes of this node.
        /// </summary>
        public List<StateChartNode> children { get; private set; }
        /// <summary>
        /// The parent node of this node. In case of a root node, this property is <c>null</c>.
        /// </summary>
        public StateChartNode parent { get; private set; }

        public List<StateChartTransition> transitions { get; private set; }

        public EnterAction enter_action { get; private set; }

        public ExitAction exit_action { get; private set; }

        public List<StateChartNode> defaults { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="csharp_sccd_compiler.StateChartNode"/> class.
        /// </summary>
        /// <param name="xml">The XML element that contains the information related to this node.</param>
        /// <param name="type">The type of node.</param>
        /// <param name="is_orthogonal">If set to <c>true</c> this node is orthogonal. (= child of a parallel state.</param>
        /// <param name="parent">The parent of this node. Defaults to null, which means the node is the root.</param>
        public StateChartNode(XElement xml, StateChartNode parent = null)
        {
            this.parent = parent;
            this.children = new List<StateChartNode>();
            this.is_root = false;
            this.is_basic = false;
            this.is_composite = false;
            this.is_history = false;
            this.is_history_deep = false;
            this.is_parallel = false;
            this.save_state_on_exit = false;

            if (xml.Name == "scxml")
            {
                this.is_root = true;
                this.is_composite = true;
            }
            else if (xml.Name == "parallel")
            {
                this.is_composite = true;
                this.is_parallel = true;
            }
            else if (xml.Name == "state")
            {
                if (xml.Element("state") != null || xml.Element("parallel") != null)
                    this.is_composite = true;
                else
                    this.is_basic = true;
                if (this.parent.is_parallel)
                {
                    if (this.is_basic)
                        throw new CompilerException("Orthogonal nodes (nodes that are immediate children of parallel nodes) can't be basic.");
                }
            }
            else if (xml.Name == "history")
            {
				this.is_history = true;

                XAttribute type_attribute = xml.Attribute("type");

                if (type_attribute != null)
                {
                    string history_type = type_attribute.Value.Trim();
                    if (history_type == "deep")
                        this.is_history_deep = true;
                    else if (history_type != "shallow")
                        throw new CompilerException("Invalid history type.");
                }
            }
            else
                return;

            this.resolveName(xml);
            this.parseConflictAttribute(xml);
            this.parseEnterActions(xml);
            this.parseExitActions(xml);

            //Parse transitions
            this.transitions = new List<StateChartTransition>();
            foreach (XElement transition_xml in xml.Elements("transition"))
                this.transitions.Add(new StateChartTransition(transition_xml, this));

            this.optimizeTransitions();
            this.generateChildren(xml);
            this.calculateDefaults(xml);
        }

        private void resolveName(XElement xml)
        {
            if (this.is_root)
            {
                this.name = "Root";
                this.full_name = "Root";
            }
            else
            {
                XAttribute name_attribute = xml.Attribute("id");
                if (name_attribute == null)
                    throw new CompilerException("Currently states without id aren't allowed.");
                this.name = name_attribute.Value.Trim();
                if (this.name == "")
                    throw new CompilerException("Currently states need an non-empty id.");
                this.full_name = string.Format("{0}_{1}", this.parent.full_name, this.name);
            }
        }

        private void parseConflictAttribute(XElement xml)
        {
            XAttribute conflict_attribute = xml.Attribute("conflict");

            if(conflict_attribute != null)
            {
                string conflict = conflict_attribute.Value.Trim();
                if (conflict == "outer")
                {
                    this.solves_conflict_outer = true;
                    return;
                }
                else if (conflict == "inner")
                {
                    this.solves_conflict_outer = false;
                    return;
                }

                if (conflict != "" && conflict != "inherit")
                    throw new CompilerException( string.Format("Unknown conflict attribute for {0}.", this.full_name));
            }
            //Do our default inherit action
            if (this.is_root || this.parent.solves_conflict_outer)
                this.solves_conflict_outer = true;
            else
                this.solves_conflict_outer = false;
        }

        private void parseEnterActions(XElement xml)
        {
            XElement onentry_xml = xml.Element("onentry");
            if (onentry_xml == null)
                this.enter_action = new EnterAction(this);
            else
            {
                this.enter_action = new EnterAction(this, onentry_xml);
                if (onentry_xml.ElementsAfterSelf("onentry").Any())
                    throw new CompilerException(string.Format("Multiple <onentry> tags detected for {0}, only 1 allowed.", this.full_name));
            }
        }

        private void parseExitActions(XElement xml)
        {
            XElement onexit_xml = xml.Element("onexit");
            if (onexit_xml == null)
                this.exit_action = new ExitAction(this);
            else
            {
                this.exit_action = new ExitAction(this, onexit_xml);
                if (onexit_xml.ElementsAfterSelf("onexit").Any())
                    throw new CompilerException(string.Format("Multiple <onexit> tags detected for {0}, only 1 allowed.", this.full_name));
            }
        }

        /// <summary>
        /// If a transition with no trigger and no guard is found then it is considered as the only transition.
        /// Otherwise the list is ordered by placing transitions having guards only first.
        /// </summary>
        private void optimizeTransitions()
        {
            List<StateChartTransition> with_trigger = new List<StateChartTransition>();
            List<StateChartTransition> only_guard = new List<StateChartTransition>();
            List<StateChartTransition> uc_and_no_guard = new List<StateChartTransition>();

            foreach( StateChartTransition transition in this.transitions)
            {
                if (transition.trigger.is_uc)
                {
                    if (transition.guard == null)
                    {
                        if (uc_and_no_guard.Count > 0)
                            throw new TransitionException("More than one transition found at a single node, that has no trigger and no guard.");
                        uc_and_no_guard.Add(transition);
                    }
                    else
                    {
                        only_guard.Add(transition);
                    }
                }
                else
                {
                    with_trigger.Add(transition);
                }
            }

            if (uc_and_no_guard.Count > 0)
                this.transitions = uc_and_no_guard;
            else
            {
                only_guard.AddRange(with_trigger);
                this.transitions = only_guard;
            }
        }

        private void generateChildren(XElement xml)
        {
            List<string> children_names = new List<string>();
            foreach (XElement child_xml in xml.Elements())
            {
                StateChartNode child = new StateChartNode(child_xml, this);
                if (!child.is_composite && !child.is_basic && !child.is_history)
                    continue;
                this.children.Add(child);

                //Check if the name of the child is valid
                if (children_names.Contains(child.name))
                    throw new CompilerException(string.Format("Found 2 equivalent state id's '{0}' as children of state '{1}'", child.name, this.full_name));
                children_names.Add(child.name);
            }
        }

        private void calculateDefaults(XElement xml)
        {
            XAttribute initial_state_attribute = xml.Attribute("initial");
            string initial_state = "";
            if (initial_state_attribute != null)
                initial_state = initial_state_attribute.Value.Trim();
        
            if (this.is_parallel)
            {
                this.defaults = (from child in this.children where !child.is_history select child).ToList();
                if (initial_state != "") 
                    throw new CompilerException(string.Format("Component <{0}> contains an initial state while being parallel.", this.full_name));   
            }
            else if (initial_state == "")
            {
                if (!this.is_basic && !this.is_history)
                {
                    if (this.children.Count == 1)
                        this.defaults = this.children;
                    else
                        throw new CompilerException(string.Format("Component <{0}> contains no default state.", this.full_name)); 
                }
            }
            else
            {
                if (this.is_basic)
                    throw new CompilerException(string.Format("Component <{0}> contains a default state while being a basic state.", this.full_name));
                this.defaults = new List<StateChartNode>();
                foreach (StateChartNode child in this.children)
                {
                    if (child.name == initial_state)
                        this.defaults.Add(child);
                }
                if (this.defaults.Count < 1)
                    throw new CompilerException(string.Format("Initial state '{0}' referred to, is missing in {1}.", initial_state, this.full_name));
                else if (this.defaults.Count > 1)
                    throw new CompilerException(string.Format("Multiple states with the name '{0}' found in {1} which is referred to as initial state.", initial_state, this.full_name));
            }
        }

        /// <summary>
        /// Returns a list representing the containment hierarchy of this node.
        /// </summary>
        /// <returns>The ancestors with node being the first element and its outermost parent (root) being the last.</returns>
        public IEnumerable<StateChartNode> getAncestors()
        {
            StateChartNode current = this;
            while ( !current.is_root)
            {
                current = current.parent;
                yield return current;
            }
        }
    
        public bool isDescendantOf(StateChartNode anc)
        {
            StateChartNode current = this;
            while(! current.is_root)
            {
                current = current.parent;
                if (object.ReferenceEquals(current,anc))
                    return true;
            }
            return false;
        }

        public bool isDescendantOrAncestorOf(StateChartNode node)
        {
            return this.isDescendantOf(node) || node.isDescendantOf(this);
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}
