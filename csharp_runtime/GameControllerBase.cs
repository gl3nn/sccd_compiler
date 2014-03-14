using System;
using System.Collections.Generic;

namespace sccdlib
{
    public class GameControllerBase : ControllerBase
    {
        public GameControllerBase (ObjectManagerBase object_manager, bool keep_running = true)
            : base(object_manager, keep_running)
        {        
        }
        
        public void update(double delta)
        {
            if (this.input_queue.Count > 0)
            {
                List<Event> next_input_queue = new List<Event>();
                foreach( Event input_event in this.input_queue)
                {
                    input_event.decTime(delta);
                    if (input_event.getTime() <= 0.0)
                    {
                        this.broadcast(input_event);
                    }
                    else
                    {
                        next_input_queue.Add(input_event);
                    }
                }
                this.input_queue = next_input_queue;
            }
            this.object_manager.stepAll(delta);
        }
    }
}

