using System;

namespace csharp_sccd_compiler
{
    public abstract class Visitable
    {
        public abstract void accept(Visitor visitor);
    }
}

