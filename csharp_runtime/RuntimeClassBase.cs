using System;
using System.Collections.Generic;

namespace sccdlib
{
    public abstract class RuntimeClassBase
    {
        
        bool active = false;
        bool state_changed = false;
        
        EventQueue events = new EventQueue();
        
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
        public void step (double delta)
        {
            if (!this.active)
                return;
            
            this.events.decreaseTime(delta);

            this.microstep();
            while (this.state_changed)
                this.microstep();
        }
        
        private void microstep ()
        {
            if (this.events.isEmpty()) {
                this.transition ();   
            } else {
                foreach( Event e in this.events.popDueEvents())
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

