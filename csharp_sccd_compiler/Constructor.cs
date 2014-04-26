using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace csharp_sccd_compiler
{
    public class Constructor : Method
    {
        public Constructor(XElement xml) : base(xml)
        {
        }

        public Constructor()
        {
            this.name = "";
            this.return_type = "";
            this.body = "";
            this.access = "public";
            this.parameters = new List<FormalParameter>();
        }
    }
}

