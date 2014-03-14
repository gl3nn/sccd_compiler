using System;
using System.Collections.Generic;

namespace sccdlib
{
    public abstract class ObjectManagerBase
    {
        ControllerBase controller;
        List<Event> event_queue = new List<Event>();
        Dictionary<RunTimeClassBase,InstanceWrapper> instances_map = new Dictionary<RunTimeClassBase,InstanceWrapper> ();
        
        public ObjectManagerBase (ControllerBase controller)
        {
            this.controller = controller;
            
        }
        
        public void addEvent (Event new_event)
        {
            this.event_queue.Add (new_event);
        }
        
        public void broadcast (Event new_event)
        {
            foreach (RuntimeClassBase instance in this.instances_map.Keys)
                instance.addEvent(new_event);
        }

        public double getEarliestEvent()
        {
            if (this.event_queue.Count > 0 )
                return this.event_queue[0].getTime ();
            return Double.MaxValue;
        }
        
        public  double getWaitTime()
        {
            //first get waiting time of the object manager which acts as statechart too
            double smallest_time = this.getEarliestEvent();
            //check all the instances
            foreach (RuntimeClassBase instance in this.instances_map.Keys)
                smallest_time = min(smallest_time, instance.getEarliestEvent());
            return smallest_time;
        }
        
        public void step (double delta)
        {
               
            if (this.event_queue.Count > 0)
            {
                List<Event> next_event_queue = new List<Event>();
                foreach( Event input_event in this.next_event_queue)
                {
                    input_event.decTime(delta);
                    if (input_event.getTime() <= 0.0)
                        this.handleEvent(input_event);
                    else
                        next_input_queue.Add(input_event);
                }
                this.event_queue = next_event_queue;
            }
        }
        
        public void stepAll (double delta)
        {
            this.step(delta);
            foreach (RuntimeClassBase instance in this.instances_map.Keys)
                instance.step(delta);
        }
    
        
        public void start ()
        {
            foreach (RuntimeClassBase instance in this.instances_map.Keys)
                instance.start(); 
        }
}

