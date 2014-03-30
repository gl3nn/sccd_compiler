using System;
using System.Collections.Generic;

namespace sccdlib
{
    public class Event
    {
        string name = "";
        string port = "";
        List<object> parameters;
        

        public Event (string name = "", string port = "", List<object> parameters = null)
        {
            this.name = name;
            this.port = port;
            this.parameters = (parameters == null ? new List<object>() : parameters);
        }

        public string getName ()
        {
            return this.name;
        }

        public string getPort ()
        {
            return this.port;
        }
        
        public List<object> getParameters ()
        {
            return this.parameters;
        }
    }
}

