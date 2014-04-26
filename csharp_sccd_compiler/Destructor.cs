using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace csharp_sccd_compiler
{
    public class Destructor : Method
    {
        public Destructor(XElement xml) : base(xml)
        {
        }
    }
}

