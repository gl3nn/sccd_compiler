using System;
using System.Collections.Generic;

namespace sccdlib
{
    /// <summary>
    /// Abstracting the event Queue. Currently uses a List to store it's content, but could have better performance when built on a priority queue.
    /// </summary>
    public class EventQueue
    {
        private class EventQueueEntry
        {
            double time_offset;
            Event e;
            
            public EventQueueEntry(Event e, double time_offset)
            {
                this.e = e;
                this.time_offset = time_offset;
            }
            
            public void decreaseTime(double offset)
            {
                this.time_offset -= offset;   
            }
            
            public Event getEvent()
            {
                return this.e;
            }
            
            public double getTime ()
            {
                return this.time_offset;
            }
            
        }
        

        List<EventQueueEntry> event_list = new List<EventQueueEntry>();
        EventQueueEntry earliest = null;
                
        public void Add (Event e, double time_offset)
        {
            EventQueueEntry entry = new EventQueueEntry(e,time_offset);
            this.event_list.Add (entry);
            ///If the newly added event is due earlier than any other, set it as earliest event
            if (this.earliest == null || time_offset < this.earliest.getTime ())
                this.earliest = entry;
        }
        
        public void decreaseTime(double offset)
        {
            foreach (EventQueueEntry e in this.event_list)
                e.decreaseTime(offset);
        }
        
        public bool isEmpty()
        {
            return this.event_list.Count == 0;
        }
        
        
        /// <summary>
        /// Gets the earliest time.
        /// </summary>
        /// <returns>
        /// The earliest time. Positive infinity if no events are present.
        /// </returns>
        public double getEarliestTime ()
        {
            if(this.earliest == null)
                return double.PositiveInfinity;
            else
                return this.earliest.getTime ();
        }
        
        public List<Event> popDueEvents ()
        {
            List<Event> result = new List<Event> ();
            if (this.earliest == null || this.earliest.getTime () > 0.0)
                //There are no events, or the earliest event isn't due, so we can already return an emtpy result
                return result;
            
            //We can be sure now that the earliest event will be removed, so we look for a new one
            this.earliest = null;
            double new_earliest_time = double.MaxValue;
            
            foreach (var entry in this.event_list) {
                if (entry.getTime() <= 0.0)
                {
                    result.Add (entry.getEvent ()); //Add all events that are due (offset less than 0) to the result
                }
                else if(entry.getTime() < new_earliest_time)
                {
                    //We seek for a new earliest event, namely the event with lowest time offset above 0.0
                    this.earliest = entry;
                    new_earliest_time = entry.getTime();
                }
            }
            this.event_list.RemoveAll ( e => e.getTime () <= 0.0 );
            return result;
        }
    }
}

