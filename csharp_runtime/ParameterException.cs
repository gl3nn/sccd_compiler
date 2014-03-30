using System;

namespace sccdlib
{
    public class ParameterException : RunTimeException
    {
        public ParameterException ()
        {
        }      
                
        public ParameterException(string message)
            : base(message)
        {
        }
    
        public ParameterException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

