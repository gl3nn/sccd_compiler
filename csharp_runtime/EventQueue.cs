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
                
        public void Add (Event e, double time_offset)
        {
            EventQueueEntry entry = new EventQueueEntry(e,time_offset);
            //We maintain a sorted stable list
            int insert_index = 0;
            for (int index = this.event_list.Count-1; index >= 0; index--)
            {
                if (this.event_list[index].getTime() <= time_offset)
                {
                    insert_index = index + 1;
                    break;
                }
            }
            this.event_list.Insert(insert_index, entry);
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
            if (this.isEmpty())
            {
                return double.PositiveInfinity;
            }
            else
            {
                return this.event_list[0].getTime();
            }
        }

        public List<Event> popDueEvents ()
        {
            List<Event> result = new List<Event> ();
            if (this.isEmpty() || this.event_list[0].getTime() > 0.0)
                //There are no events, or the earliest event isn't due, so we can already return an emtpy result
                return result;

            int index = 0;
            while (index < this.event_list.Count && this.event_list[index].getTime() <= 0.0)
            {
                result.Add(this.event_list[index].getEvent()); //Add all events that are due (offset less than 0) to the result
                index++;
            }
            this.event_list.RemoveRange(0, result.Count);
            return result;
        }
    }
}

