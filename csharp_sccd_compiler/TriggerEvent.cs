using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class TriggerEvent
    {
        public bool is_after { get; private set; }
        public bool is_uc { get; private set; }

        public int after_index { get; set; }
        public List<FormalEventParameter> parameters { get; private set; }

        public string event_name { get; set; }
        public string port { get; private set; }
        public Expression after_expression { get; private set; }

        public TriggerEvent(XElement xml)
        {
            after_index = -1; //Initial value

            //Parse "port" attribute
            XAttribute port_attribute = xml.Attribute("port");
            if (port_attribute == null || port_attribute.Value.Trim() == "")
                this.port = null;
            else
                this.port = port_attribute.Value.Trim();

            //Parse "after" attribute
            XAttribute after_attribute = xml.Attribute("after");
            if (after_attribute == null || after_attribute.Value.Trim() == "")
            {
                this.after_expression = null;
                this.is_after = false;
            }
            else
            {
                this.after_expression = new Expression(after_attribute.Value.Trim());
                if (this.port != null)
                    throw new CompilerException("An after event can not have a port defined.");
                this.is_after = true;

            }

            //parse "event" attribute
            XAttribute event_name_attribute = xml.Attribute("event");
            if (event_name_attribute == null || event_name_attribute.Value.Trim() == "")
            {
                this.event_name = null;
                if (this.port != null)
                    throw new CompilerException("A transition without event can not have a port.");
                if (!this.is_after)
                    this.is_uc = true;
            }
            else
            {
                this.event_name = event_name_attribute.Value.Trim();
                if (this.is_after)
                    throw new CompilerException("Cannot have both the event and after attribute set for a transition.");
            }
         
            this.parameters = new List<FormalEventParameter>();
            foreach (XElement parameter_xml in xml.Elements("parameter"))
            {
                this.parameters.Add(new FormalEventParameter(parameter_xml));
            }
            if(this.parameters.Count > 0 && (this.is_after || this.is_uc ))
                throw new CompilerException("AFTER and unconditional events can not have parameters defined.");
        }
    }
}
