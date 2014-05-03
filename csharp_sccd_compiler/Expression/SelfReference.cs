using System;

namespace csharp_sccd_compiler
{
    public class SelfReference : ExpressionPart
    {
        public SelfReference()
        {
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

