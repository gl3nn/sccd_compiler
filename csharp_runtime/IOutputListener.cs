using System;

namespace sccdlib
{
    public interface IOutputListener
    {
        void add (Event output_event);
        Event fetch ();
    }
}

