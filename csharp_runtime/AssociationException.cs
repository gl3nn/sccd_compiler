using System;

namespace sccdlib
{
    public class AssociationException : RunTimeException
    {
        public AssociationException ()
        {
        }
        
        public AssociationException(string message)
            : base(message)
        {
        }
    
        public AssociationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
  
}

