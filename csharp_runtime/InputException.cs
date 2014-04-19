using System;

namespace sccdlib
{
    public class InputException : RunTimeException
    {
        public InputException ()
        {
        }
        
        public InputException(string message)
            : base(message)
        {
        }
    
        public InputException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
  
}

