using System;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class Log : SubAction
    {
        public string message { get; private set; }

        public Log(XElement xml)
        {
            this.message = xml.Value.Trim();
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}