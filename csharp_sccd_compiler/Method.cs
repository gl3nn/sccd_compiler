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

        public Class parent_class { get; protected set; }

        public List<FormalParameter> parameters { get; protected set; }

        protected Method(Class parent_class)
        {
            this.name = "";
            this.return_type = "";
            this.access = "public";
            this.body = "";

            this.parent_class = parent_class;
            this.parameters = new List<FormalParameter>();
        }

        public Method(XElement xml, Class parent_class, bool is_constructor_or_destructor)
        {
            this.parent_class = parent_class;

            if (!is_constructor_or_destructor)
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
            } else
            {
                this.name = "";
                this.return_type = "";
            }

            //Set access level
            XAttribute access_attribute = xml.Attribute("access");
            if (access_attribute == null || access_attribute.Value.Trim() == "")
                this.access = "public"; //default value
            else
                this.access = access_attribute.Value.Trim();

            //Set body
            XElement[] bodies = xml.Elements("body").ToArray();
            if (bodies.Length > 1)
                throw new CompilerException("Multiple bodies found.");
            if (bodies.Length == 1)
                this.body = bodies [0].Value;
            else
                this.body = "";

            //Set parameters
            this.parameters = new List<FormalParameter>();
            foreach (XElement parameter_xml in xml.Elements("parameter"))
            {
                this.parameters.Add(new FormalParameter(parameter_xml));
            }
        }

        public Method(XElement xml, Class parent_class): this(xml, parent_class, false)
        {
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}