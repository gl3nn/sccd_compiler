using System;

namespace csharp_sccd_compiler
{
    abstract public class Visitable
    {
        public Visitable()
        {

        }

        public virtual void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

