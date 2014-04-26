using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class StateChartTransition : Visitor.Visitable
    {
        public TriggerEvent trigger { get; private set; }

        public StateChartNode parent { get; private set; }

        public Expression guard { get; private set; }

        public StateChartTransition(XElement xml, StateChartNode parent)
        {
            this.parent = parent;
            this.trigger = new TriggerEvent(xml);
            XAttribute guard_attribute = xml.Attribute("cond");
            if (guard_attribute == null || guard_attribute.Value.Trim() == "")
                this.guard = new Expression(guard_attribute.Value.Trim());
            else
                this.guard = null;


            /*
        self.xml = xml_element
        self.parent_node = parent
        self.trigger = TriggerEvent(self.xml)
        guard_string = self.xml.get("cond","").strip()
        if guard_string != "" : 
            self.guard = Expression(guard_string)
        else :
            self.guard = None
        target_string = self.xml.get("target","").strip()
        if target_string == "" :
            raise CompilerException("Transition from <" + self.parent_node.full_name + "> has empty target.")
        self.target = StateReference(target_string)
        
        self.action = Action(self.xml)
        
        self.enter_nodes = None #Ordered list of nodes to be entered upon taking the transition, set by the path calculator
        self.exit_nodes = None #Ordered list of nodes to be exited upon taking the transition, set by the path calculator
             */
        }
    }
}

