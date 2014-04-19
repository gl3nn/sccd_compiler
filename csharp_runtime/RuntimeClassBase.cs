using System;
using System.Collections.Generic;

namespace sccdlib
{
    public abstract class RuntimeClassBase
    {
        
        protected bool active = false;
        protected bool state_changed = false;
        protected EventQueue events = new EventQueue();
        protected ControllerBase controller;
        protected ObjectManagerBase object_manager;
        protected Dictionary<int,double> timers;

        public RuntimeClassBase ()
        {
        }
        
        public void addEvent (Event input_event, double time_offset = 0.0)
        {
            this.events.Add (input_event, time_offset);
        }
        
        
        public double getEarliestEventTime ()
        {
            return this.events.getEarliestTime();   
        }
        
        /// <summary>
        /// Execute statechart
        /// </summary>
        /// <param name='delta'>
        /// Time passed since last step.
        /// </param>
        public void step(double delta)
        {
            if (!this.active)
                return;
            
            
            if (this.timers != null && this.timers.Count > 0)
            {
                var next_timers = new Dictionary<int,double>();
                foreach(KeyValuePair<int,double> pair in this.timers)
                {
                    double time = pair.Value - delta;
                    if (time <= 0.0)
                        this.addEvent ( new Event("_" + pair.Key + "after"));
                    else
                        next_timers[pair.Key] = time;
                }
                this.timers = next_timers;
            }
                    
            this.events.decreaseTime(delta);

            this.microstep();
            while (this.state_changed)
                this.microstep();
        }
        
        private void microstep ()
        {
            var due = this.events.popDueEvents();
            if (due.Count == 0) {
                this.transition ();   
            } else {
                foreach( Event e in due)
                    this.transition (e);
            }
        }
        
        protected abstract void transition (Event e = null);
        
        public virtual void start ()
        {
            this.active = true;
        }
        
    }
}

