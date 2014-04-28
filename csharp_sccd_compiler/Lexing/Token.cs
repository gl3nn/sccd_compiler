using System;

namespace csharp_sccd_compiler
{
    public class Token
    {
        public enum Type
        {
            SLASH,
            LBRACKET,
            RBRACKET,
            COMMA,
            DOT,
            NUMBER,
            WORD,
            QUOTED,
            WHITESPACE,
            UNKNOWN
        }

        public Type type { get; private set; }

        public string val { get; private set; }

        public int pos { get; private set; }

        public Token(Type token_type, string value, int pos)
        {
            this.type = token_type;
            this.val = value;
            this.pos = pos;
        }

        public override string ToString()
        {
            return string.Format("{0}({1}) at {2}", Enum.GetName(typeof(Type), this.type), this.val, this.pos);
        }
    }
}

