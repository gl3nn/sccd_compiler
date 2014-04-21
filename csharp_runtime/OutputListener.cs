using System;
using System.Collections.Generic;

namespace sccdlib
{
    public class OutputListener : IOutputListener
    {
        Queue<Event> queue = new Queue<Event>();
        List<string> ports = new List<string>();
        
        public OutputListener (string[] port_names)
        {
            foreach (string port_name in port_names)
            {
                this.ports.Add (port_name);
            }
        }
        
        public void add (Event output_event)
        {
            if (this.ports.Count == 0 || this.ports.Contains (output_event.getPort ())) {
                this.queue.Enqueue (output_event);
            }
        }
                
        public Event fetch ()
        {
            if (this.queue.Count > 0)
                return this.queue.Dequeue ();
            return null;
        }
    }
}

