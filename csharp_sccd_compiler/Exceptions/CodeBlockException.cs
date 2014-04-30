using System;

namespace csharp_sccd_compiler
{
    public class CodeBlockException : CompilerException
    {
        public CodeBlockException ()
        {
        }

        public CodeBlockException(string message)
            : base(message)
        {
        }

        public CodeBlockException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

