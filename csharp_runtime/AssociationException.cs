using System;

namespace sccdlib
{
    public class AssociationException : Exception
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

