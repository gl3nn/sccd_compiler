using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace csharp_sccd_compiler
{
    public class Association : Visitable
    {
        /// <summary>
        /// Minimum cardinality of the association.
        /// </summary>
        public int min { get; private set; }
        /// <summary>
        /// Maximum cardinality of the association.
        /// </summary>
        /// <value>N is represented as -1.</value>
        public int max { get; private set; }

        public string to_class { get; private set; } // TODO perhaps try to replace the string by the actual class with a visitor? That's an extra error check.

        public string name { get; private set; }

        public Association(XElement xml)
        {
            XAttribute class_name_attribute = xml.Attribute("class");
            if (class_name_attribute == null)
                throw new CompilerException("Association missing class attribute.");
            this.to_class = class_name_attribute.Value.Trim();
            if (this.to_class == "")
                throw new CompilerException("Association has empty class attribute.");

            if (Constants.Reserved.Contains(this.to_class))
                throw new CompilerException(string.Format("Reserved word '{0}' used as class attribute for association.", this.to_class));

            XAttribute min_card_attribute = xml.Attribute("min");
            if (min_card_attribute == null)
                this.min = 0; //default value
            else
            {
                try
                {
                    this.min = Convert.ToInt32(min_card_attribute.Value);
                    if(this.min < 0)
                        throw new FormatException();            
                }
                catch(FormatException)
                {
                    throw new CompilerException("Faulty minimum cardinality value in association.");
                }
                catch(OverflowException)
                {
                    throw new CompilerException("Minimum cardinality of association is too large.");
                }
            }

            XAttribute max_card_attribute = xml.Attribute("max");
            if (max_card_attribute == null)
                this.max = -1; //default value TODO:use maxvalue?
            else
            {
                try
                {
                    this.max = Convert.ToInt32(max_card_attribute.Value);
                    if(this.max < this.min)
                        throw new FormatException();                 
                }
                catch(FormatException)
                {
                    throw new CompilerException("Faulty maximum cardinality value in association.");
                }
                catch(OverflowException)
                {
                    throw new CompilerException("Maximum cardinality of association is too large.");
                }
            }

            XAttribute association_name_attribute = xml.Attribute("name");
            if (association_name_attribute == null)
                throw new CompilerException("Association missing name attribute.");
            this.name = association_name_attribute.Value.Trim();
            if (this.name == "")
                throw new CompilerException("Associaion has empty name attribute.");

            if (Constants.Reserved.Contains(this.name))
                throw new CompilerException(string.Format("Reserved word '{0}' used as class attribute for association.", this.name));
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

