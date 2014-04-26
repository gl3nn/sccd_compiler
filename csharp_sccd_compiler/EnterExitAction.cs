using System;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class EnterExitAction : Visitor.Visitable
    {
        public StateChartNode parent { get; private set; }

        public Action action { get; private set; }

        public EnterExitAction(StateChartNode parent, XElement xml = null)
        {
            this.parent = parent;
            if (xml != null)
                this.action = new Action(xml);
            else
                this.action = null;
        }
    }

    public class EnterAction : EnterExitAction
    {
        public EnterAction(StateChartNode parent, XElement xml = null) : base(parent,xml)
        {
        }
    }

    public class ExitAction : EnterExitAction
    {
        public ExitAction(StateChartNode parent, XElement xml = null) : base(parent,xml)
        {
        }
    }
}

