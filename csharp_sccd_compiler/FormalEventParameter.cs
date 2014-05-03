using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class FormalEventParameter : Visitable
    {
        public string name { get; private set; }
        public string type { get; private set; }

        public FormalEventParameter(XElement xml)
        {
            //Parse "name" attribute
            XAttribute name_attribute = xml.Attribute("name");
            if (name_attribute == null)
                throw new CompilerException("Missing name for formal event parameter.");
            this.name = name_attribute.Value.Trim();
            if (this.name == "")
                throw new CompilerException("Empty name for formal event parameter.");

            //Parse "type" attribute
            XAttribute type_attribute = xml.Attribute("type");
            if (type_attribute == null)
                throw new CompilerException("Missing type for formal event parameter.");
            this.type = type_attribute.Value.Trim();
            if (this.type == "")
                throw new CompilerException("Empty type for formal event parameter.");
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

