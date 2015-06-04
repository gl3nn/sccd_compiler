using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using csharp_sccd_compiler;

namespace SCCDEditor{
    public class SCCDUtility
    {
        public static void onClassRename(XElement renamed_class, string new_name, string old_name)
        {
            foreach (XElement class_xml in renamed_class.Parent.Elements("class"))
            {
                if (class_xml == renamed_class)
                    continue;
                XElement relationships = class_xml.Element("relationships");
                if (relationships == null)
                    continue;
                foreach (XElement association in relationships.Elements("association"))
                {
                    if (association.Attribute("class").Value == old_name)
                        association.Attribute("class").Value = new_name;

                }
            }

        }

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

        public static string createTargetString(XElement target_xml)
        {
            List<string> path = new List<string>();
            while (!SCCDUtility.hasName(target_xml, "scxml"))
            {
                path.Add(target_xml.Attribute("id").Value);
                target_xml = target_xml.Parent;
            }
            path.Reverse();
            return "/" + string.Join("/", path.ToArray());
        }

        public static XElement getTransitionTarget(XElement root_state, XElement source_state, string target)
        {
            List<XElement> target_nodes = new List<XElement>();
            XElement current_node = null; //Will be used to find the target state(s)
            Stack<XElement> split_stack = new Stack<XElement>(); // used for branching
            Lexer lexer = new Lexer();
            lexer.setInput(target);
            
            foreach (Token token in lexer.iterateTokens())
            {
                if (current_node == null) //current_node is not set yet or has been reset, the CHILD token can now have a special meaning
                {
                    if (token.type == Token.Type.SLASH) 
                    {
                        current_node = root_state; //Root detected
                        continue;
                    }
                    else
                        current_node = source_state;
                }
                
                if (token.type == Token.Type.DOT)
                {
                    Token next_token = lexer.nextToken();//Advance to next token
                    if (next_token == null || next_token.type == Token.Type.SLASH)
                        continue;//CURRENT operator "." detected
                    else if (next_token.type == Token.Type.DOT)
                    {
                        next_token = lexer.nextToken();//Advance to next token
                        if (next_token == null || next_token.type == Token.Type.SLASH)
                        {
                            current_node = current_node.Parent; //PARENT operator ".." detected
                            if (current_node == null)
                                throw new StateReferenceException(string.Format("Illegal use of PARENT \"..\" operator at position {0} in state reference. Root of statechart reached.", token.pos));
                        }
                        else
                            throw new StateReferenceException(string.Format("Illegal use of PARENT \"..\" operator at position {0} in state reference.", token.pos));
                    }
                    else
                        throw new StateReferenceException(string.Format("Illegal use of CURRENT \"..\" operator at position {0} in state reference.", token.pos));
                }
                else if (token.type == Token.Type.SLASH)
                    continue;
                else if (token.type == Token.Type.WORD)
                {
                    //Trying to advance to next child state
                    string child_name = token.val;
                    bool found = false;
                    List<XElement> child_states = current_node.Elements("state").ToList();
                    child_states.AddRange(current_node.Elements("parallel"));
                    child_states.AddRange(current_node.Elements("history"));
                    foreach (XElement child in child_states)
                    {
                        if (child.Attribute("id").Value == child_name)
                        {
                            found = true;
                            current_node = child;
                            break;
                        }
                    }
                    if (!found)
                        throw new StateReferenceException(string.Format("Refering to non exiting node at position {0} in state reference.", token.pos));
                }
                else if (token.type == Token.Type.LBRACKET)
                    split_stack.Push(current_node);
                else if (token.type == Token.Type.RBRACKET)
                {
                    if (split_stack.Count > 0)
                        split_stack.Pop();
                    else
                        throw new StateReferenceException(string.Format("Invalid token at position {0} in state reference.", token.pos));
                }
                else if (token.type == Token.Type.COMMA)
                {
                    target_nodes.Add(current_node);
                    if (split_stack.Count > 0)
                        current_node = split_stack.Peek();
                    else
                        current_node = null;
                }
                else
                    throw new StateReferenceException(string.Format("Invalid token at position {0} in state reference.", token.pos));
            }
            
            if (split_stack.Count != 0 || current_node == null) //RB missing or extra COMMA
                throw new StateReferenceException("State reference ends unexpectedly.");

            target_nodes.Add(current_node);
            
            if (target_nodes.Count == 0)
                throw new StateReferenceException("Meaningless state reference.");

            return target_nodes [0];
        }
    }
}