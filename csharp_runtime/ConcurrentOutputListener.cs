using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace sccdlib
{
    public class ConcurrentOutputListener : IOutputListener
    {
        ConcurrentQueue<Event> queue = new ConcurrentQueue<Event>();
        List<string> ports = new List<string>();
        
        public ConcurrentOutputListener (string[] port_names = new string[]{})
        {
            foreach (string port_name in port_names)
            {
                this.ports.Add (port_name);
            }
        }
        
        public void addOutput (Event output_event)
        {
            if (this.ports.Count == 0 || this.ports.Contains (output_event.getPort ())) {
                this.queue.Enqueue (Event);
            }
        }
                
        public Event fetchOutput ()
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

