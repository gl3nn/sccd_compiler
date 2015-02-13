using System;

namespace SCCDEditor
{
    public class SGUIException : Exception
    {
        public SGUIException ()
        {
        }
        
        public SGUIException(string message)
            : base(message)
        {
        }
        
        public SGUIException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
