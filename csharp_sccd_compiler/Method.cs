using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class Method : Visitable
    {
        public string name { get; protected set; }
        public string return_type { get; protected set; }

        public string access { get; protected set; }
        public string body { get; protected set; }

        public List<FormalParameter> parameters { get; protected set; }

        protected Method()
        {
        }

        public Method(XElement xml)
        {
            //Set name
            XAttribute name_attribute = xml.Attribute("name");
            if (name_attribute == null)
                throw new CompilerException("Missing method name.");
            this.name = name_attribute.Value.Trim();
            if (this.name == "")
                throw new CompilerException("Empty method name.");
            if (Constants.Reserved.Contains(this.name))
                throw new CompilerException(string.Format("Reserved word '{0}' used as method name.", this.name));

            //Set return type
            XAttribute return_attribute = xml.Attribute("type");
            if (return_attribute == null)
                throw new CompilerException("Missing method type.");
            this.return_type = return_attribute.Value.Trim();
            if (this.return_type == "")
                throw new CompilerException("Empty method type.");

            //Set access level
            XAttribute access_attribute = xml.Attribute("access");
            if (access_attribute == null || access_attribute.Value.Trim() == "")
                this.access = "public"; //default value
            else
                this.access = access_attribute.Value.Trim();

            //Set body
            XAttribute body_attribute = xml.Attribute("body");
            if (body_attribute == null)
                this.body = "";
            else
                this.body = body_attribute.Value;

            //Set parameters
            this.parameters = new List<FormalParameter>();
            foreach (XElement parameter_xml in xml.Elements("parameter"))
            {
                this.parameters.Add(new FormalParameter(parameter_xml));
            }
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}