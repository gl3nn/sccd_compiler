using System;
using System.Collections.Generic;

namespace sccdlib
{
    public abstract class ControllerBase
    {
        protected ObjectManagerBase object_manager;
        protected bool keep_running;
        protected bool done = false;
        protected List<string> input_ports;
        protected EventQueue input_queue;

        protected List<string> output_ports;
        protected List<OutputListener> output_listeners;



        public ControllerBase (ObjectManagerBase object_manager, bool keep_running = true)
        {
            this.object_manager = object_manager;
            this.keep_running = keep_running;
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
    
        public virtual void addInput(string event_name, string port, double time = 0.0, List<object> parameters = null)
        {
            //Check event_name and port to be not empty?
            this.input_queue.Add(new Event(event_name, port, parameters), time);
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
    
        public void addEventList(List<Event> event_list)
        {
            foreach (Event input_event in event_list)
            {   
                this.input_queue.Add(input_event,0.0);
            }
        }
    }
}

