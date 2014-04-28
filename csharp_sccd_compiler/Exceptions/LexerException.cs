using System;

namespace csharp_sccd_compiler
{
    public class LexerException : CompilerException
    {
        public LexerException ()
        {
        }

        public LexerException(string message)
            : base(message)
        {
        }

        public LexerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}



