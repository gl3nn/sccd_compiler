using System;

namespace csharp_sccd_compiler
{
    public class TransitionException : CompilerException
    {
        public TransitionException ()
        {
        }

        public TransitionException(string message)
            : base(message)
        {
        }

        public TransitionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

