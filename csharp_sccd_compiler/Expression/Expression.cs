using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace csharp_sccd_compiler
{
    public class Expression : Visitable
    {
        private static Lexer lexer = new Lexer(false, true);

        public List<ExpressionPart> expression_parts { get; private set; }

        public Expression(string input_string)
        {
            if (input_string == "")
                throw new CompilerException("Empty Expression.");
            this.parse(input_string);
        }

        protected void parse(string input_string, string[] dont_parse = null)
        {
            this.expression_parts = new List<ExpressionPart>();
            Expression.lexer.setInput(input_string);
            string processed_bare_expression = "";
            if (dont_parse == null)
                dont_parse = new string[] { };

            foreach (Token token in Expression.lexer.iterateTokens())
            {
                ExpressionPart created_part = null;

                if (token.type == Token.Type.WORD)
                {
                    if (dont_parse.Contains(token.val))
                        throw new CompilerException(string.Format("Macro \"{0}\" not allowed here.",token.val));
                    else if (token.val == Constants.SELF_REFERENCE_SEQ)
                        created_part = new SelfReference();
                    else if (token.val == Constants.INSTATE_SEQ)
                    {
                        created_part = this.parseInStateCall();
                        if (created_part == null)
                            throw new CompilerException(string.Format("Illegal use of \"{0}\" macro.", Constants.INSTATE_SEQ));
                    }
                }

                if (created_part == null)
                    processed_bare_expression += token.val;
                else
                {
                    if (processed_bare_expression != "")
                    {
                        this.expression_parts.Add( new ExpressionPartString(processed_bare_expression));
                        processed_bare_expression = "";
                    }
                    this.expression_parts.Add(created_part);
                }
            }

            //Process part of input after the last created macro object
            if (processed_bare_expression != "")
                this.expression_parts.Add( new ExpressionPartString(processed_bare_expression));
        }

        private InStateCall parseInStateCall()
        {
            Token token = Expression.lexer.nextToken();
            if (token == null || token.type != Token.Type.LBRACKET)
                return null;
            token = Expression.lexer.nextToken();
            if (token == null || token.type != Token.Type.QUOTED)
                return null;
            InStateCall result = new InStateCall(token.val.Substring(1, token.val.Length - 2));
            token = Expression.lexer.nextToken();
            if (token == null || token.type != Token.Type.RBRACKET)
                return null;
            return result;
        }

        public override void accept(Visitor visitor)
        {
            foreach (ExpressionPart expression_part in this.expression_parts)
                expression_part.accept(visitor);
        }
    }
}

