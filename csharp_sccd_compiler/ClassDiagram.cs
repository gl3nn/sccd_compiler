using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace csharp_sccd_compiler
{
	public class ClassDiagram : Visitable
	{
        public string model_name { get; private set; }

        public string model_author { get; private set; }

        public string model_description { get; private set; }

        public List<string> class_names { get; private set; }

        public List<string> inports { get; private set; }

        public List<string> outports { get; private set; }

        public string top_section { get; private set; }

        public List<Class> classes { get; private set; }

        public Class default_class { get; private set; }

		public ClassDiagram (string input_file_path)
        {
            XElement root = XDocument.Load(input_file_path).Root;
            XAttribute name_attribute = root.Attribute("name");
            if (name_attribute != null && name_attribute.Value.Trim() != "")
                this.model_name = name_attribute.Value.Trim();
            XAttribute author_attribute = root.Attribute("author");
            if (author_attribute != null && author_attribute.Value.Trim() != "")
                this.model_author = author_attribute.Value.Trim();
            XElement description_element = root.Element("description");
            if (description_element != null && description_element.Value.Trim() != "")
                model_description = description_element.Value;

            this.class_names = new List<string>();
			foreach (XElement class_xml in root.Elements("class"))
            {
                XAttribute class_name_attribute = class_xml.Attribute("name");
                if (class_name_attribute == null)
                    throw new CompilerException("Missing class name.");
                string class_name = class_name_attribute.Value.Trim();
                if (class_name == "")
                    throw new CompilerException("Empty class name.");
                if (this.class_names.Contains(class_name))
                    throw new CompilerException(string.Format("Found 2 classes with the same name '{0}'.", class_name));
                this.class_names.Add(class_name);
            }

            if (this.class_names.Count == 0)
                throw new CompilerException("Found no classes to compile.");

			this.inports = new List<string> ();
            foreach (XElement inport_xml in root.Elements("inport"))
            {
                XAttribute inport_name_attribute = inport_xml.Attribute("name");
                if (inport_name_attribute == null)
                    throw new CompilerException("Missing inport name.");
                string inport_name = inport_name_attribute.Value.Trim();
                if (inport_name == "")
                    throw new CompilerException("Empty inport name.");
                if (this.inports.Contains(inport_name))
                    throw new CompilerException(string.Format("Found 2 inports with the same name '{0}'.", inport_name));
                this.inports.Add(inport_name);
            }

			this.outports = new List<string> ();
            foreach (XElement outport_xml in root.Elements("outport"))
            {
                XAttribute outport_name_attribute = outport_xml.Attribute("name");
                if (outport_name_attribute == null)
                    throw new CompilerException("Missing outport name.");
                string outport_name = outport_name_attribute.Value.Trim();
                if (outport_name == "")
                    throw new CompilerException("Empty outport name.");
                if (this.outports.Contains(outport_name))
                    throw new CompilerException(string.Format("Found 2 outports with the same name '{0}'.", outport_name));
                this.outports.Add(outport_name);
            }

            List<XElement> top_elements = root.Elements("top").ToList();
            if (top_elements.Count == 1 && top_elements[0].Value.Trim() != "")
                this.top_section = top_elements[0].Value;
            else if (top_elements.Count > 1)
                throw new CompilerException("Class diagram can only have one <top> element.");

            this.classes = new List<Class>(); 
            List<Class> default_classes = new List<Class>(); 

            foreach (XElement class_xml in root.Elements("class"))
            {
                Class processed_class = null;
                try
                {
                    processed_class = new Class(class_xml);
                }
                catch(CompilerException e)
                {
                    throw new CompilerException(string.Format("Class <{0}> failed compilation.", class_xml.Attribute("name").Value), e);
                }

                Logger.displayInfo(string.Format("Class <{0}> has been successfully loaded.", processed_class.name));
                this.classes.Add(processed_class);
                if (processed_class.is_default)
                    default_classes.Add(processed_class);
            }

            if (default_classes.Count != 1)
            {
                if (this.classes.Count == 1)
                {
                    Logger.displayInfo(string.Format("Only one class given. Using <{0}> as the default class.", this.classes[0].name));
                    this.default_class = this.classes[0];
                }
                else
                    throw new CompilerException("Provide one and only one default class to instantiate on start up.");
            }
            else
                this.default_class = default_classes[0];
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
	}
}