using System;
using System.Collections.Generic;

namespace sccdlib
{
    public class Event
    {
        string name = "";
        double time = 0.0;
        string port = "";
        List<object> parameters;
        

        public Event ()
        {
        }

        public string getName ()
        {
            return this.name;
        }

        public double getTime ()
        {
            return this.time;
        }

        public string getPort ()
        {
            return this.port;
        }
        
        public List<object> getParameters ()
        {
            return this.parameters;
        }
        
        public void decTime(double delta)
        {
            this.time -= delta;
        }


    }
}

