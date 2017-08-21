using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonInterpreter
{
    /// <summary>
    /// Scans and tokenizes a python code file
    /// </summary>
    public class Lexer
    {
        /// <summary>
        /// The code file that is being tokenized
        /// </summary>
        private StreamReader _File;

        public Lexer(Stream stream)
        {
            _File = new StreamReader(stream);
        }

        /// <summary>
        /// Returns the next token in the file
        /// </summary>
        public Token GetNextToken()
        {
            //While the next character is not the end of the file
            while (_File.Peek() != -1)
            {
                char nextChar = (char)_File.Peek();

                if (char.IsWhiteSpace(nextChar))
                {
                    _SkipWhitespace();
                    continue;
                }

                if (nextChar == '=')
                {
                    _File.Read();
                    return new Token(TokenType.ASN, "=");
                }

                if (nextChar == '{')
                {
                    _File.Read();
                    return new Token(TokenType.LCRLY, "{");
                }

                if (nextChar == '}')
                {
                    _File.Read();
                    return new Token(TokenType.RCRLY, "}");
                }

                if (nextChar == ',')
                {
                    _File.Read();
                    return new Token(TokenType.COMMA, ",");
                }

                if (nextChar == ';')
                {
                    _File.Read();
                    return new Token(TokenType.SEMI, ";");
                }

                if (char.IsDigit(nextChar))
                {
                    return _ParseDigits();
                }

                if (nextChar == '/')
                {
                    _File.Read();
                    return new Token(TokenType.DIV, "/");
                }

                if (nextChar == '*')
                {
                    _File.Read();
                    return new Token(TokenType.MUL, "*");
                }

                if (nextChar == '+')
                {
                    _File.Read();
                    return new Token(TokenType.ADD, "+");
                }

                if (nextChar == '-')
                {
                    _File.Read();
                    return new Token(TokenType.SUB, "-");
                }

                if (nextChar == '(')
                {
                    _File.Read();
                    return new Token(TokenType.LPRN, "(");
                }

                if (nextChar == ')')
                {
                    _File.Read();
                    return new Token(TokenType.RPRN, ")");
                }

                if (char.IsLetter(nextChar))
                {
                    return _TokenizeID();
                }

                throw new NotSupportedException("Unexpected token " + nextChar);
            }

            return new Token(TokenType.EOF, "");
        }

        private Token _TokenizeID()
        {
            StringBuilder sb = new StringBuilder();
            
            while (char.IsLetterOrDigit((char)(_File.Peek())))
            {
                sb.Append((char)_File.Read());
            }


            if(sb.ToString() == "int")
            {
                return new Token(TokenType.INT, "int");
            }
            if (sb.ToString() == "float")
            {
                return new Token(TokenType.FLOAT, "float");
            }

            return new Token(TokenType.ID, sb.ToString());
        }

        private Token _ParseDigits()
        {
            StringBuilder sb = new StringBuilder();

            while (char.IsDigit((char)_File.Peek()) || (char)_File.Peek() == '.')
            {
                sb.Append((char)_File.Read());
            }

            int res;
            float fres;

            if (int.TryParse(sb.ToString(), out res))
            {
                return new Token(TokenType.INT_CONST, sb.ToString());
            }
            else if (float.TryParse(sb.ToString(), out fres))
            {
                return new Token(TokenType.FLOAT_CONST, sb.ToString());
            }

            throw new Exception("Unable to parse input");
        }

        private string _ParseInteger()
        {
            StringBuilder val = new StringBuilder();

            while (char.IsDigit((char)_File.Peek()))
            {
                val.Append((char)_File.Read());
            }

            return val.ToString();
        }

        private void _SkipWhitespace()
        {
            while (char.IsWhiteSpace((char)_File.Peek()))
            {
                _File.Read();
            }
        }
    }
}
