using System;
using System.Collections.Generic;

namespace sccdlib
{
    public abstract class ControllerBase
    {
        protected ObjectManagerBase object_manager;
        protected bool done = false;
        protected List<string> input_ports = new List<string>();
        protected EventQueue input_queue = new EventQueue();

        protected List<string> output_ports = new List<string>();
        protected List<IOutputListener> output_listeners = new List<IOutputListener>();

        public ControllerBase ()
        {
        }

        protected void addInputPort(string port_name)
        {
            this.input_ports.Add(port_name);
        }
        
        protected void addOutputPort(string port_name)
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

        public void outputEvent(Event output_event)
        {
            foreach (IOutputListener listener in this.output_listeners)
            {
                listener.add(output_event);
            }
        }
        
        public IOutputListener addOutputListener(string[] ports)
        {
            IOutputListener listener = this.createOutputListener(ports);
            this.output_listeners.Add(listener);
            return listener;
        }
        
        protected virtual IOutputListener createOutputListener (string[] ports)
        {
            return new OutputListener(ports);   
        }
        
        public virtual void addInput(Event input_event, double time_offset = 0.0)
        {   
            if ( input_event.getName() == "" )
                throw new InputException("Input event can't have an empty name.");
            
            if ( !this.input_ports.Contains (input_event.getPort()) )
                throw new InputException("Input port mismatch.");
            
            this.input_queue.Add(input_event, time_offset);
        }
    
        public virtual void addEventList(List<Tuple<Event,double>> event_list)
        {
            foreach (Tuple<Event,double> event_tuple in event_list)
            {   
                this.addInput (event_tuple.Item1, event_tuple.Item2);
            }
        }
        
        public ObjectManagerBase getObjectManager ()
        {
            return this.object_manager;
        }
    }
}

