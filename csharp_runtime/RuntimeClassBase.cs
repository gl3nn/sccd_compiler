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
        protected Dictionary<int,double> timers = null;

        public RuntimeClassBase ()
        {
        }
        
        public void addEvent (Event input_event, double time_offset = 0.0)
        {
            this.events.Add (input_event, time_offset);
        }
        
        
        public double getEarliestEventTime ()
        {
            if (this.timers != null)
            {
                double smallest_timer_value = double.PositiveInfinity;
                foreach (double timer_value in this.timers.Values)
                {
                    if (timer_value < smallest_timer_value)
                        smallest_timer_value = timer_value;
                }
                return Math.Min(this.events.getEarliestTime(), smallest_timer_value); 
            }
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

            this.events.decreaseTime(delta);

            if (this.timers != null && this.timers.Count > 0)
            {
                var next_timers = new Dictionary<int,double>();
                foreach(KeyValuePair<int,double> pair in this.timers)
                {
                    double new_time = pair.Value - delta;
                    if (new_time <= 0.0)
                        this.addEvent (new Event("_" + pair.Key + "after"), new_time);
                    else
                        next_timers[pair.Key] = new_time;
                }
                this.timers = next_timers;
            }

            this.microstep();
            while (this.state_changed)
                this.microstep();
        }
        
        private void microstep ()
        {
            List<Event> due = this.events.popDueEvents();
            if (due.Count == 0) {
                this.transition ();   
            } else {
                foreach (Event e in due)
                {
                    this.transition(e);
                }
            }
        }
        
        protected abstract void transition (Event e = null);
        
        public virtual void start ()
        {
            this.active = true;
        }
        
    }
}

