using System;

namespace csharp_sccd_compiler
{
    public class UnprocessedException : CompilerException
    {
		public UnprocessedException ()
        {
        }

		public UnprocessedException(string message)
            : base(message)
        {
        }

		public UnprocessedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

