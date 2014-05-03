using System;
using System.Collections.Generic;

namespace csharp_sccd_compiler
{
    public class StateReference : Visitable
    {
        public string state_reference_string { get; private set; }

        /// <summary>
        /// Populated later by State Linker
        /// </summary>
        public List<StateChartNode> target_nodes { get; set; }

        public StateReference(string state_reference_string)
        {
            this.state_reference_string = state_reference_string;
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

