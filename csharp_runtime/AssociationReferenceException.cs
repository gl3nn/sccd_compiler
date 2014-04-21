using System;

namespace sccdlib
{
    public class AssociationReferenceException : RunTimeException
    {
        public AssociationReferenceException ()
        {
        }
        
        public AssociationReferenceException(string message)
            : base(message)
        {
        }
    
        public AssociationReferenceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

