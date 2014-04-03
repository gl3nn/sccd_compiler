from utils import Enum


TokenType = Enum("SLASH",
                 "LB",
                 "RB",
                 "COMMA",
                 "CURRENT",
                 "PARENT",
                 "NUMBER",
                 "WORD",
                 "QUOTED"
                )

class Token(object):    
    
    """ A simple Token structure. Token type, value and position.
    """
    def __init__(self, token_type, val, pos):
        self.type = token_type
        self.val = val
        self.pos = pos

    def __str__(self):
        return '%s(%s) at %s' % (TokenType.name_of(self.type), self.val, self.pos)


class LexerError(Exception):
    def __init__(self, pos):
        self.pos = pos
        
class Lexer(object):
    
    def __init__(self):
        self.single_rules = {
            '/': TokenType.SLASH,
            '(': TokenType.LB,
            ')': TokenType.RB,
            ',': TokenType.COMMA    
        }


    def input(self, buf):
        """ Initialize the lexer with a buffer as input.
        """
        self.buf = buf
        self.pos = 0
        self.buflen = len(buf)

    def nextToken(self):
        """ Return the next token (a Token object) found in the
            input buffer. None is returned if the end of the
            buffer was reached.
            In case of a lexing error (the current chunk of the
            buffer matches no rule), a LexerError is raised.
        """
        self.skipNonTokens()
        if self.pos >= len(self.buf):
            return None

        #c part of next token
        c = self.buf[self.pos]
        
        #Token that starts with . should be treated differently
        if c == '.' :
            if (self.pos+1 == self.buflen) or (self.buf[self.pos+1] == '/') :
                token = Token(TokenType.CURRENT,".",self.pos)
                self.pos += 1;
                return token
            elif self.buf[self.pos+1] == '.' :
                if (self.pos+2 == self.buflen) or (self.buf[self.pos+2] == '/') :
                    token = Token(TokenType.PARENT,"..",self.pos)
                    self.pos += 2;
                    return token
                else :
                    raise LexerError(self.pos+2)

            else :
                raise LexerError(self.pos+1)
        else :
            #check if it is an operator
            result_type = self.single_rules.get(c,None)
            if result_type is not None :
                token = Token(result_type, c, self.pos)
                self.pos += 1
                return token
            else : #not an operator
                if (self.isAlpha(c)) :
                    return self.processIdentifier()
                elif (self.isDigit(c)) :
                    return self.processNumber()
                elif ( c == "'" or c == '"') :
                    return self.processQuote()

        # if we're here, no rule matched
        raise LexerError(self.pos)

    def tokens(self):
        """ Returns an iterator to the tokens found in the buffer.
        """
        while 1:
            tok = self.nextToken()
            if tok is None: break
            yield tok
            
    def skipNonTokens(self):
        while (self.pos < self.buflen) : 
            c = self.buf[self.pos]
            if (c == ' ' or c == '\t' or c == '\r' or c == '\n') :
                self.pos += 1
            else :
                break
            
    def isAlpha(self, c):
        return c.isalpha() or c == '_';
    
    def isAlphaNum(self, c):
        return c.isalnum() or c == '_';
    
    def isDigit(self, c):
        return c.isdigit();
    
    def processNumber(self):
        nextpos = self.pos + 1
        while (nextpos < self.buflen) and (self.isDigit(self.buf[nextpos])) :
            nextpos += 1;
        token = Token(TokenType.NUMBER, self.buf[self.pos:nextpos], self.pos)
        self.pos = nextpos
        return token
    
    def processIdentifier(self):
        nextpos = self.pos + 1
        while (nextpos < self.buflen) and (self.isAlphaNum(self.buf[nextpos])) :
            nextpos += 1;
        token = Token(TokenType.WORD, self.buf[self.pos:nextpos], self.pos)
        self.pos = nextpos
        return token
    
    def processQuote(self):
        # this.pos points at the opening quote. Find the ending quote.
        end_index = self.buf.find(self.buf[self.pos], self.pos + 1)
    
        if (end_index == -1) :
            raise LexerError(self.pos)
        token = Token(TokenType.QUOTED, self.buf[self.pos+1:end_index], self.pos)

        self.pos = end_index + 1;
        return token;