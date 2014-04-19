using System;
using System.Collections.Generic;

namespace sccdlib
{
    public class Event
    {
        string name = "";
        string port = "";
        object[] parameters;
        

        public Event (string name = "", string port = "", object[] parameters = null)
        {
            this.name = name;
            this.port = port;
            this.parameters = (parameters == null ? new object[] {} : parameters);
        }

        public string getName ()
        {
            return this.name;
        }

        public string getPort ()
        {
            return this.port;
        }
        
        public object[] getParameters ()
        {
            return this.parameters;
        }
        
        public override string ToString()
        {
            return string.Format("(event name : {0}; port : {1}; parameters : [{2}])", this.name, this.port, string.Join(", ", this.parameters));
        }
    }
}

