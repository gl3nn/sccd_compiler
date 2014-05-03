using System;

namespace csharp_sccd_compiler
{
	public class InStateCall : ExpressionPart
    {
        public StateReference state_reference { get; private set; }

        public InStateCall(string state_reference_string)
        {
            this.state_reference = new StateReference(state_reference_string);
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

