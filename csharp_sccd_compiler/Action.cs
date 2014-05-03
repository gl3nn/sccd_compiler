using System;
using System.Collections.Generic;
using System.Xml.Linq;


namespace csharp_sccd_compiler
{
    public class Action : Visitable
    {
        public List<SubAction> sub_actions { get; private set; }

        public Action(XElement xml)
        {
            this.sub_actions = new List<SubAction>();
            foreach (XElement sub_action_xml in xml.Elements())
            {
                SubAction sub_action = null;
                if (sub_action_xml.Name == "raise")
                    sub_action = new RaiseEvent(sub_action_xml);
                else if (sub_action_xml.Name == "script")
                    sub_action = new Script(sub_action_xml);
                else if (sub_action_xml.Name == "log")
                    sub_action = new Log(sub_action_xml);
                else if (sub_action_xml.Name == "assign")
                    sub_action = new Assign(sub_action_xml);
                else if (sub_action_xml.Name != "parameter")      
                    throw new CompilerException(string.Format("Invalid subaction <{0}>.", sub_action_xml.Name));

                if (sub_action != null)
                    this.sub_actions.Add(sub_action);
            }
        }

        public override void accept(Visitor visitor)
        {
            foreach (SubAction sub_action in this.sub_actions)
                sub_action.accept(visitor);
        }
    }
}

