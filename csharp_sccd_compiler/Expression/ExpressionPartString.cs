using System;

namespace csharp_sccd_compiler
{
    public class ExpressionPartString : ExpressionPart
    {
        public string value { get; private set; }

        public ExpressionPartString(string value)
        {
            this.value = value;
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

