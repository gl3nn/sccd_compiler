using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class Attribute : Visitable
    {
        public string name { get; private set; }
        public string type { get; private set; }
        public string init_value { get; private set; }

        public Attribute(XElement xml)
        {
            XAttribute name_attribute = xml.Attribute("name");
            if (name_attribute == null)
                throw new CompilerException("Missing attribute name.");
            this.name = name_attribute.Value.Trim();
            if (this.name == "")
                throw new CompilerException("Empty attribute name.");
            if (Constants.Reserved.Contains(this.name))
                throw new CompilerException(string.Format("Reserved word '{0}' used as attribute name.", this.name));

            XAttribute type_attribute = xml.Attribute("type");
            if (type_attribute == null)
                throw new CompilerException("Missing attribute type.");
            this.type = type_attribute.Value.Trim();
            if (this.type == "")
                throw new CompilerException("Empty attribute type.");

            XAttribute init_attribute = xml.Attribute("init-value");
            if (init_attribute == null || init_attribute.Value == "")
                this.init_value = null;
            else
                this.init_value = init_attribute.Value;
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

