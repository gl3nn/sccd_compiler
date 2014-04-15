using System;
using System.Collections.Generic;

namespace sccdlib
{
    public abstract class ControllerBase
    {
        protected ObjectManagerBase object_manager;
        protected bool done = false;
        protected List<string> input_ports;
        protected EventQueue input_queue = new EventQueue();

        protected List<string> output_ports;
        protected List<OutputListener> output_listeners;

        public ControllerBase ()
        {
        }

        public void addInputPort(string port_name)
        {
            this.input_ports.Add(port_name);
        }
        
        public void addOutputPort(string port_name)
        {
            this.output_ports.Add(port_name);
        }

        public void broadcast(Event new_event)
        {
            this.object_manager.broadcast(new_event);
        }
        
        public virtual void start()
        {
            this.object_manager.start();
        }
    
        public virtual void stop()
        {
        }

        private void outputEvent(Event output_event)
        {
            foreach (OutputListener listener in this.output_listeners)
            {
                listener.addOutput(output_event);
            }
        }
        
        public OutputListener addOutputListener(string[] ports)
        {
            OutputListener listener = new OutputListener(ports);
            this.output_listeners.Add(listener);
            return listener;
        }
        
        public virtual void addInput(Event input_event, double time_offset = 0.0)
        {
            this.input_queue.Add(input_event, time_offset);
        }
    
        public virtual void addEventList(List<Tuple<Event,double>> event_list)
        {
            foreach (Tuple<Event,double> event_tuple in event_list)
            {   
                this.input_queue.Add(event_tuple.Item1, event_tuple.Item2);
            }
        }
        
        public ObjectManagerBase getObjectManager ()
        {
            return this.object_manager;
        }
    }
}

