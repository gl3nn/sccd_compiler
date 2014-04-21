using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace sccdlib
{
    public class ConcurrentOutputListener : IOutputListener
    {
        ConcurrentQueue<Event> queue = new ConcurrentQueue<Event>();
        List<string> ports = new List<string>();
        
        public ConcurrentOutputListener (string[] port_names)
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
            Event fetched_event;
            bool success = this.queue.TryDequeue (out fetched_event);
            if (success) {
                return fetched_event;
            }
            return null;
        }
    }
}

