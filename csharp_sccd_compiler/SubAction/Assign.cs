using System;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class Assign : SubAction
    {
        public LValue lvalue { get; private set; }

        public Expression expression { get; private set; }

        public Assign(XElement xml)
        {
            XAttribute ident_attribute = xml.Attribute("ident");
            if (ident_attribute == null)
                throw new ActionException("Missing \"ident\" attribute for assignment.");
            this.lvalue = new LValue(ident_attribute.Value.Trim());

            XAttribute expression_attribute = xml.Attribute("expr");
            if (expression_attribute == null)
                throw new ActionException("Missing \"expr\" attribute for assignment.");
            this.expression = new Expression(expression_attribute.Value.Trim());
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}