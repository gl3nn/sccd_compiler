using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace csharp_sccd_compiler
{
    public class Constructor : Method
    {
        // Old code where multiple super calls were allowed. (Mulitple inheritance)
        // Dictionary<string, List<Expression>> super_class_parameters = new Dictionary<string, List<Expression>>();

        public List<Expression> super_class_parameters { get; private set; }

        public Constructor(XElement xml, Class parent_class) : base(xml, parent_class, true)
        {
            /*
            // Old code where multiple super calls were allowed. (Mulitple inheritance)
            foreach (XElement super_xml in xml.Elements("super"))
            {
                XAttribute class_name_attribute = super_xml.Attribute("class");
                if (class_name_attribute == null)
                    throw new CompilerException("Missing class attribute in <super> tag.");
                string class_name = class_name_attribute.Value.Trim();
                if (class_name == "")
                    throw new CompilerException("Empty class attribute in <super> tag.");
                if (this.super_class_parameters.ContainsKey(class_name))
                    throw new CompilerException("2 <super> tags in the same constructor, contain the same class.");
                this.super_class_parameters[class_name] = new List<Expression>();
                foreach (XElement parameter_xml in super_xml.Elements("parameter"))
                {
                    XAttribute expr_attribute = parameter_xml.Attribute("expr");
                    if (expr_attribute == null || expr_attribute.Value.Trim() == "")
                        throw new ActionException("<parameter> in <super> detected without \"expr\" attribute.");
                    this.super_class_parameters[class_name].Add( new Expression(expr_attribute.Value.Trim()));
                }
            }*/
            XElement[] super_calls_array = xml.Elements("super").ToArray();
            if (super_calls_array.Length > 1)
                throw new CompilerException("Only one super call is allowed.");
            else if (super_calls_array.Length == 1)
            {
                this.super_class_parameters = new List<Expression>();
                foreach (XElement parameter_xml in super_calls_array[0].Elements("parameter"))
                {
                    XAttribute expr_attribute = parameter_xml.Attribute("expr");
                    if (expr_attribute == null || expr_attribute.Value.Trim() == "")
                        throw new ActionException("<parameter> in <super> detected without \"expr\" attribute.");
                    this.super_class_parameters.Add( new Expression(expr_attribute.Value.Trim()));
                }
            }
        }

        public Constructor(Class parent_class): base(parent_class)
        {
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

