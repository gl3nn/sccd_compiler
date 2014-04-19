using System;
using System.Collections.Generic;
using sccdlib;

namespace csharp_tests
{
    public class TestEvent
    {
        string name;
        string port;
        List<string> parameters;

        public TestEvent(string name, string port, List<string> parameters)
        {
            this.name = name;
            this.port = port;
            this.parameters = parameters;
        }

        public bool matches(Event output_event)
        {
            if (output_event == null)
                return false;
            if (output_event.getName() != this.name)
                return false;
            if (output_event.getPort() != this.port)
                return false;
            List<object> compare_parameters = new List<object>(output_event.getParameters());
            if (this.parameters.Count != compare_parameters.Count)
                return false;
            for (int i = 0; i < this.parameters.Count; i++)
            {
                if (this.parameters [i] != compare_parameters[i].ToString())
                    return false;
            }
            return true;
        }
    
        public override string ToString()
        {
            return string.Format("(event name : {0}; port : {1}; parameters : [{2}])", this.name, this.port, string.Join(", ", this.parameters));
        }
    }
}

