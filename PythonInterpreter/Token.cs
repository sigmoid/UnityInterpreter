using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonInterpreter
{
    /// <summary>
    /// The different token types this interpreter supports
    /// </summary>
    public enum TokenType {
        INT,            //Integer type
        FLOAT,          //Float type
        INT_CONST,      //An integer
        FLOAT_CONST,    //A float
        MUL,            //Multiply (*)
        DIV,            //Divide (/)
        ADD,            //Add (+)
        SUB,            //Subtract (-)
        EOF,            //End of file
        LPRN,           //Left Parenthesis
        RPRN,           //Right Parenthesis
        LCRLY,          //Left curly brace {
        RCRLY,          //Right curly brace }
        SEMI,           //Semicolon
        ASN,            //Assignment (=)
        ID,             //Variable id
        FUNCDEF,        //Function definition
        ARG,            //An Argument
        COMMA           // ','
    };

    public class Token
    {
        public TokenType Type;
        public string Value;

        public Token(TokenType type, string val)
        {
            Console.WriteLine("Create " + type + " token " + val);
            this.Type = type;
            this.Value = val;
        }
    }
}
