using System;

namespace sccdlib
{
    public class RunTimeException : Exception
    {
        public RunTimeException ()
        {
        }
        
        public RunTimeException(string message)
            : base(message)
        {
        }
    
        public RunTimeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

