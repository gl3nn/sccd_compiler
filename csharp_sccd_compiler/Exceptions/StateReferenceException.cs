using System;

namespace csharp_sccd_compiler
{
    public class StateReferenceException : CompilerException
    {
        public StateReferenceException ()
        {
        }

        public StateReferenceException(string message)
            : base(message)
        {
        }

        public StateReferenceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

