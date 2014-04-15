using System;
using System.Collections.Generic;

namespace sccdlib
{
    public class GameControllerBase : ControllerBase
    {
        public GameControllerBase ()
            : base()
        {        
        }
        
        public void update(double delta)
        {  
            this.input_queue.decreaseTime(delta);
            foreach(Event e in this.input_queue.popDueEvents())
                this.broadcast (e);
            this.object_manager.stepAll(delta);
        }
    }
}

