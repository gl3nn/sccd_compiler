using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class Class : Visitable
    {
        public string name { get; private set; }
        public bool is_default { get; private set; }

        public List<Constructor> constructors { get; private set; }
        public List<Destructor> destructors { get; private set; }
        public List<Method> methods { get; private set; }
        public List<Attribute> attributes { get; private set; }
        public List<Association> associations { get; private set; }

        public StateChart statechart { get; private set; }
        public string super_class { get; private set; }

        public Class(XElement xml)
        {
            this.name = xml.Attribute("name").Value;
            XAttribute default_attribute = xml.Attribute("default");
            if (default_attribute != null && default_attribute.Value.ToLower() == "true")
            {
                this.is_default = true;
            }
            else
            {
                this.is_default = false;
            }
            foreach(XElement attribute_xml in xml.Elements("attribute"))
            {
                this.attributes.Add(new Attribute(attribute_xml));
            }
            foreach(XElement method_xml in xml.Elements("method"))
            {
                this.processMethod(method_xml);
            }
            if (this.destructors.Count > 1)
                throw new CompilerException("Multiple destructors defined.");

            if (this.constructors.Count == 0)
                this.constructors.Add(new Constructor());


            var associations = new List<XElement>();
            var inheritances = new List<XElement>();
            foreach (XElement relationships_xml in xml.Elements("relationships"))
            {
                associations.AddRange(relationships_xml.Elements("assocation"));
                inheritances.AddRange(relationships_xml.Elements("inheritance"));
            }

            foreach( XElement association_xml in associations)
                this.associations.Add(new Association(association_xml));

            if(inheritances.Count > 1)
                throw new CompilerException("Multiple inheritance detected which is not supported.");
            if (inheritances.Count == 1)
                this.super_class = inheritances[0].Attribute("class").Value;
            else
                this.super_class = null;

            XElement[] statecharts = xml.Elements("scxml").ToArray();
            if (statecharts.Length > 1)
                throw new CompilerException("Multiple statecharts found.");
            if (statecharts.Length == 1)
                this.statechart = new StateChart(statecharts[0]);
        }

        private void processMethod(XElement method_xml)
        {
            XAttribute method_name_attribute = method_xml.Attribute("name");
            if (method_name_attribute == null)
                throw new CompilerException("Missing method name.");
            string method_name = method_name_attribute.Value;
            if (method_name == "")
                throw new CompilerException("Empty method name.");
            if (Constants.Reserved.Contains(method_name))
                throw new CompilerException(string.Format("Reserved word '{0}' used as method name.", method_name));
            if (method_name == this.name)
                this.constructors.Add(new Constructor(method_xml));
            else if (method_name == string.Concat("~",this.name))
                this.destructors.Add(new Destructor(method_xml));
            else
                this.methods.Add(new Method(method_xml));
        }
    }
}
/*
    def processMethod(self, method_xml) :
        name = method_xml.get("name", "")
        if name == self.name :
            self.constructors.append(Constructor(method_xml, self))
        elif name == '~' + self.name:
            self.destructors.append(Destructor(method_xml, self))
        else :
            if name in reserved:
                raise CompilerException("Reserved word \"" + name + "\" used as method in class <" + self.name + ">.")
            self.methods.append( Method(method_xml, self))

    def processAttribute(self, attribute_xml):
        attribute = Attribute(attribute_xml)
        if attribute.name in reserved:
            raise CompilerException("Reserved word \"" + attribute.name + "\" used as variable in class <" + self.name + ">.")

        self.attributes.append(attribute)


*/