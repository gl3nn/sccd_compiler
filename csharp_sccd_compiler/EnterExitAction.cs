using System;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public abstract class EnterExitAction : Visitable
    {
        public StateChartNode parent { get; private set; }

        public Action action { get; private set; }

        public EnterExitAction(StateChartNode parent, XElement xml)
        {
            this.parent = parent;
            if (xml != null)
                this.action = new Action(xml);
            else
                this.action = null;
        }

        public EnterExitAction(StateChartNode parent): this(parent, null) {}

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }

    public class EnterAction : EnterExitAction
    {
        public EnterAction(StateChartNode parent, XElement xml) : base(parent, xml) {}

        public EnterAction(StateChartNode parent) : this(parent, null) {}

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }

    public class ExitAction : EnterExitAction
    {
        public ExitAction(StateChartNode parent, XElement xml) : base(parent, xml) {}

        public ExitAction(StateChartNode parent) : this(parent, null) {}

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

