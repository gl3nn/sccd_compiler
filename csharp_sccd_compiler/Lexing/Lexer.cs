using System;
using System.Collections.Generic;

namespace csharp_sccd_compiler
{
    public class Lexer
    {
        bool skip_white_space;
        bool accept_unknown_tokens;
        static Dictionary<char, Token.Type> single_rules = new Dictionary<char, Token.Type>{
            { '/', Token.Type.SLASH },
            { '(', Token.Type.LBRACKET },
            { ')', Token.Type.RBRACKET },
            { ',', Token.Type.COMMA },
            { '.', Token.Type.DOT },
        };

        string buf;
        int pos;

        public Lexer( bool skip_white_space = true, bool accept_unknown_tokens = false)
        {
            this.accept_unknown_tokens = accept_unknown_tokens;
            this.skip_white_space = skip_white_space;
        }

        /// <summary>
        /// Initialize the lexer with a buffer as input.
        /// </summary>
        public void setInput(string buffer)
        {
            this.buf = buffer;
            this.pos = 0;
        }

        /// <summary>
        /// Return the next token (a Token object) found in the input buffer. None is returned if the end of the buffer was reached.
        /// In case of a lexing error (the current chunk of the buffer matches no rule), a LexerException is raised.
        /// </summary>
        public Token nextToken()
        {
            if (this.skip_white_space)
                this.skipWhiteSpace();
            if (this.pos >= this.buf.Length)
                return null;

            char c = this.buf[this.pos]; //first char of next token

            Token.Type result_type;
            if (Lexer.single_rules.TryGetValue(c, out result_type)) //check if it is an operator
            {
                Token token = new Token(result_type, c.ToString(), this.pos);
                this.pos += 1;
                return token;
            }
            else //not an operator
            {
                if (this.isAlpha(c))
                    return this.processWord();
                else if (this.isDigit(c))
                    return this.processNumber();
                else if (c == '\'' || c == '"')
                    return this.processQuote();
                else if (this.isWhiteSpace(c))
                    return this.processWhiteSpace();
            }
            //if we're here, no rule matched
            if (this.accept_unknown_tokens)
            {
                Token token = new Token(Token.Type.UNKNOWN, c.ToString(), this.pos);
                this.pos += 1;
                return token;
            }
            throw new LexerException(string.Format("Invalid character at position {0}.", this.pos));
        }

        public IEnumerable<Token> iterateTokens()
        {
            while (true)
            {
                Token tok = this.nextToken();
                if (tok == null)
                    break;
                yield return tok;
            }
        }

        private void skipWhiteSpace()
        {
            while (this.pos < this.buf.Length)
            {
                if (this.isWhiteSpace(this.buf[this.pos]))
                    this.pos += 1;
                else
                    break; 
            }
        }

        private bool isWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\r' || c == '\n';
        }

        private bool isAlpha(char c)
        {
            return char.IsLetter(c) || c == '_';
        }

        private bool isDigit(char c)
        {
            return char.IsDigit(c);
        }

        private bool isAlphaNum(char c)
        {
            return this.isAlpha(c) || this.isDigit(c);
        }

        private Token processWhiteSpace()
        {
            int nextpos = this.pos + 1;
            while ( nextpos < this.buf.Length && this.isWhiteSpace(this.buf[nextpos]))
                nextpos += 1;
            Token token = new Token(Token.Type.WHITESPACE, this.buf.Substring(this.pos, nextpos-this.pos), this.pos);
            this.pos = nextpos;
            return token;
        }

        private Token processNumber()
        {
            int nextpos = this.pos + 1;
            while ( nextpos < this.buf.Length && this.isDigit(this.buf[nextpos]))
                nextpos += 1;
            Token token = new Token(Token.Type.NUMBER, this.buf.Substring(this.pos, nextpos-this.pos), this.pos);
            this.pos = nextpos;
            return token;
        }

        private Token processWord()
        {
            int nextpos = this.pos + 1;
            while ( nextpos < this.buf.Length && this.isAlphaNum(this.buf[nextpos]))
                nextpos += 1;
            Token token = new Token(Token.Type.WORD, this.buf.Substring(this.pos, nextpos-this.pos), this.pos);
            this.pos = nextpos;
            return token;
        }

        private Token processQuote()
        {
            //this.pos points at the opening quote. Find the ending quote.
            int end_index = this.buf.IndexOf(this.buf[this.pos], this.pos + 1);
            if (end_index == -1)
                throw new LexerException(string.Format("Missing matching quote for the quote at position {0}.", this.pos));

            Token token = new Token(Token.Type.QUOTED, this.buf.Substring(this.pos, end_index-this.pos+1), this.pos);
            this.pos = end_index + 1;
            return token;
        }
    }
}