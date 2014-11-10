using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace csharp_sccd_compiler
{
    public class Destructor : Method
    {
        public Destructor(XElement xml, Class parent_class) : base(xml, parent_class, true)
        {
        }

        public Destructor(Class parent_class): base(parent_class)
        {
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

