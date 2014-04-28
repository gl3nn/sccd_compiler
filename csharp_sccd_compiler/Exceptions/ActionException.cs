using System;

namespace csharp_sccd_compiler
{
    public class ActionException : CompilerException
    {
        public ActionException ()
        {
        }

        public ActionException(string message)
            : base(message)
        {
        }

        public ActionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

