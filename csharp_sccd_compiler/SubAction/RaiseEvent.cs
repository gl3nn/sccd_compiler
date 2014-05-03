using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class RaiseEvent : SubAction
    {
        public enum Scope {
            UNKNOWN_SCOPE,
            LOCAL_SCOPE,
            BROAD_SCOPE,
            OUTPUT_SCOPE,
            NARROW_SCOPE,
            CD_SCOPE
        };

        public string event_name { get; private set; }

        public string target { get; private set; }

        public string port { get; private set; }

        public Scope scope { get; private set; }

        public List<Expression> parameters { get; private set; }

        public RaiseEvent(XElement xml)
        {
            XAttribute event_attribute = xml.Attribute("event");
            if (event_attribute == null)
                throw new ActionException("Missing \"event\" attribute for <raise> action.");
            this.event_name = event_attribute.Value.Trim();
            if (this.event_name == "")
                throw new ActionException("Empty \"event\" attribute for <raise> action.");

            XAttribute target_attribute = xml.Attribute("target");
            if (target_attribute == null || target_attribute.Value.Trim() == "")
                this.target = null;
            else
                this.target = target_attribute.Value.Trim();

            XAttribute port_attribute = xml.Attribute("port");
            if (port_attribute == null || port_attribute.Value.Trim() == "")
                this.port = null;
            else
                this.port = port_attribute.Value.Trim();

            string scope_string;
            XAttribute scope_attribute = xml.Attribute("scope");
            if (scope_attribute == null || scope_attribute.Value.Trim() == "")
                scope_string = null;
            else
                scope_string = scope_attribute.Value.Trim();

            if (scope_string == null)
            {
                //Calculate scope depending on present attributes
                if (this.target != null && this.port != null)
                    throw new ActionException("Both target and port attribute detected for <raise> action without a scope defined.");
                else if (this.port != null)
                    this.scope = Scope.OUTPUT_SCOPE;
                else if (this.target != null)
                    this.scope = Scope.NARROW_SCOPE;
                else
                    this.scope = Scope.LOCAL_SCOPE;
            }
            else if (scope_string == "local")
                this.scope = Scope.LOCAL_SCOPE;
            else if (scope_string == "broad")
                this.scope = Scope.BROAD_SCOPE;
            else if (scope_string == "output")
                this.scope = Scope.OUTPUT_SCOPE;
            else if (scope_string == "narrow")
                this.scope = Scope.NARROW_SCOPE;
            else if (scope_string == "cd")
                this.scope = Scope.CD_SCOPE;    
            else
                throw new ActionException(string.Format("Illegal scope attribute \"{0}\"; needs to be one of the following : local, broad, narrow, output, cd or nothing.", scope_string));  
        
            if (this.scope == Scope.LOCAL_SCOPE || this.scope == Scope.BROAD_SCOPE || this.scope == Scope.CD_SCOPE)
            {
                if (this.target != null)
                    throw new ActionException("<raise> target detected, not matching with scope.");
                if (this.port != null)
                    throw new ActionException("<raise> port detected, not matching with scope. Ignored.");
            }
            if (this.scope == Scope.NARROW_SCOPE && this.port != null)
                throw new ActionException("<raise> port detected, not matching with scope");
            if (this.scope == Scope.OUTPUT_SCOPE && this.target != null)
                throw new ActionException("Raise event target detected, not matching with scope. Ignored.");     

            this.parameters = new List<Expression>();
            foreach (XElement parameter_xml in xml.Elements("parameter"))
            {
                XAttribute expr_attribute = parameter_xml.Attribute("expr");
                if (expr_attribute == null || expr_attribute.Value.Trim() == "")
                    throw new ActionException("<parameter> in <raise> detected without \"value\" attribute.");
                this.parameters.Add( new Expression(expr_attribute.Value.Trim()));
            }
        }

        public override void accept(Visitor visitor)
        {
            visitor.visit (this);
        }
    }
}

