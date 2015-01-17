using System;

namespace SCCDEditor
{
    public class GUIException : Exception
    {
        public GUIException ()
        {
        }
        
        public GUIException(string message)
            : base(message)
        {
        }
        
        public GUIException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
