using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace csharp_sccd_compiler
{
    public class Constructor : Method
    {
        public Constructor(XElement xml) : base(xml)
        {
        }

        public Constructor(string class_name)
        {
            this.name = class_name;
            this.access = "public";
            this.parameters = new List<FormalParameter>();
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

