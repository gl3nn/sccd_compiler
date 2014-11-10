using System;
using System.Collections.Generic;

namespace sccdlib
{
    public interface IRuntimeClass
    {
        void addEvent(Event input_event, double time_offset);
        void addEvent(Event input_event);
        double getEarliestEventTime();
        void step(double delta);
        void start();
        void stop();
        
    }
}

