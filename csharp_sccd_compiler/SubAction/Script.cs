using System;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class Script : SubAction
    {
        public string code { get; private set; }

        public Script(XElement xml)
        {
            this.code = xml.Value.Trim();
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}