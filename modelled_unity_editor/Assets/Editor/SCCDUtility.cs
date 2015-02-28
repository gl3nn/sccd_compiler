using System.Xml.Linq;

namespace SCCDEditor{
    public class SCCDUtility
    {
        public static void assureAttribute(XElement element, string attribute_name, string initial_value = "")
        {
            XAttribute attribute = element.Attribute(attribute_name);
            if (attribute == null)
            {
                element.Add(new XAttribute(attribute_name, initial_value));
            }
        }

        public static bool hasName(XElement element, string name)
        {
            return element.Name.LocalName == name;
        }

        public static void assureChild(XElement element, string child_name)
        {
            XElement child = element.Element(child_name);
            if (child == null)
            {
               element.Add(new XElement(child_name));
            }
        }
    }
}