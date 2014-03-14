using System;

namespace sccdlib
{
    public interface IOutputListener
    {
        void addOutput (Event output_event);
        Event fetchOutput ();
    }
}

