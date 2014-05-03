using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace csharp_sccd_compiler
{
    public class FormalParameter : Visitable
    {
        public string name { get; protected set; }
        public string type { get; protected set; }
        public string default_value { get; protected set; }

        public FormalParameter(string name, string type, string default_value = null)
        {
            this.name = name;
            this.type = type;
            this.default_value = default_value;
        }

        public FormalParameter(XElement xml)
        {
            //Set name
            XAttribute name_attribute = xml.Attribute("name");
            if (name_attribute == null)
                throw new CompilerException("Missing formal parameter name.");
            this.name = name_attribute.Value.Trim();
            if (this.name == "")
                throw new CompilerException("Empty formal parameter name.");
            if (Constants.Reserved.Contains(this.name))
                throw new CompilerException(string.Format("Reserved word '{0}' used as formal parameter name.", this.name));

            //Set type
            XAttribute type_attribute = xml.Attribute("type");
            if (type_attribute == null)
                throw new CompilerException("Missing formal parameter type.");
            this.type = type_attribute.Value.Trim();
            if (this.type == "")
                throw new CompilerException("Empty formal parameter type.");

            //Set default value
            XAttribute default_attribute = xml.Attribute("default");
            if (default_attribute == null || default_attribute.Value.Trim() == "")
                this.default_value = null;
            else
                this.default_value = default_attribute.Value.Trim();
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

