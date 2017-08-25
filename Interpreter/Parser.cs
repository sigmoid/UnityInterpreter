using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    /// <summary>
    /// Utilizes a lexer to turn tokens into an AST for interpreting or compiling.
    /// This parser's grammar is a stripped-down version of C
    /// </summary>
    public class Parser
    {
        private Lexer _Lexer;

        private Token _CurrentToken;

        public Parser(Lexer lex)
        {
            _Lexer = lex;
            _CurrentToken = lex.GetNextToken();
        }

        /// <summary>
        /// Parse the file supplied to the lexer and return an AST tree representing
        /// the program.
        /// </summary>
        /// <returns></returns>
        public AST Parse()
        {
            return new AST(_Program());
        }

        /// <summary>
        /// Checks that the next token is of the correct type. Advance the lexer if 
        /// it is, and throws an exception if it isn't.
        /// </summary>
        /// <param name="_type"></param>
        private void _Eat(TokenType _type)
        {
            if (_CurrentToken.Type == _type)
            {
                _CurrentToken = _Lexer.GetNextToken();
                return;
            }
            else
                throw new Exception("Unexpected token. Expected: \"" + _type.ToString() + "\" Got: \"" + _CurrentToken.Type + "\"");
        }

        private ASTNode _Program()
        {
            return _FunctionDefinition();
        }

        private ASTFunctionDefNode _FunctionDefinition()
        {
            if(_CurrentToken.Value != "function")
            {
                throw new Exception("Token: " + _CurrentToken.Value + "not expected. Expected 'function'");
            }

            //Eat "function"
            _Eat(TokenType.ID);

            //First token is the name of the function
            string funcName = _CurrentToken.Value;
            ASTFunctionDefNode ret = new ASTFunctionDefNode(funcName);
            ret.Value = _CurrentToken; 

            //Eat the function name token
            _Eat(TokenType.ID);

            _Eat(TokenType.LPRN);

            ret.Paramaters = _Paramaters();

            _Eat(TokenType.RPRN);

            ret.Body = _CompoundStatement();

            return ret;
        }

        private List<ASTArgNode> _Paramaters()
        {
            List<ASTArgNode> nodes = new List<ASTArgNode>();
            ASTTypeNode type = _Paramater();
            nodes.Add(new ASTArgNode(type));

            //As long as there is a comma after this paramater
            while (_CurrentToken.Value == ",")
            {
                _Eat(TokenType.COMMA);
                type = _Paramater();
                nodes.Add(new ASTArgNode(type));
            }

            return nodes;
        }

        private ASTTypeNode _Paramater()
        {
            ASTTypeNode ret = _Type();
            _Eat(TokenType.ID);
            return ret;
        }

        private ASTCompoundNode _CompoundStatement()
        {
            _Eat(TokenType.LCRLY);
            List<ASTNode> tmpNodes;
            tmpNodes = _StatementList();
            _Eat(TokenType.RCRLY);

            ASTCompoundNode node = new ASTCompoundNode();
            node.Children = tmpNodes;

            return node;
        }

        private ASTNode _Expression()
        {
            //Parse the first term (Required)
            var tmpNode = _Term();

            //Parse any additional following Terms (Optional)
            while (_CurrentToken.Type == TokenType.ADD ||
                _CurrentToken.Type == TokenType.SUB)
            {
                var opToken = _CurrentToken;
                _Eat(_CurrentToken.Type);

                tmpNode = new ASTBinopNode(tmpNode, opToken, _Term());
            }

            return tmpNode;
        }

        private ASTNode _Term()
        {
            //Parse the first factor
            var tmpNode = _Factor();

            //Parse following factors (optional)
            while (_CurrentToken.Type == TokenType.MUL ||
                _CurrentToken.Type == TokenType.DIV)
            {
                var opToken = _CurrentToken;
                _Eat(_CurrentToken.Type);

                tmpNode = new ASTBinopNode(tmpNode, opToken, _Factor());
            }

            return tmpNode;
        }

        private ASTNode _Factor()
        {
            //If this factor starts with an operator parse it as a unary operation
            if (_CurrentToken.Type == TokenType.ADD || _CurrentToken.Type == TokenType.SUB)
            {
                var tmpToken = _CurrentToken;
                _Eat(_CurrentToken.Type);
                return new ASTUnaryNode(tmpToken, _Factor());
            }

            else if (_CurrentToken.Type == TokenType.INT_CONST)
            {
                var tmpToken = _CurrentToken;
                _Eat(TokenType.INT_CONST);
                return new ASTNumNode(tmpToken);
            }
            else if (_CurrentToken.Type == TokenType.FLOAT_CONST)
            {
                var tmpToken = _CurrentToken;
                _Eat(TokenType.FLOAT_CONST);
                return new ASTNumNode(tmpToken);
            }

            //If the current token is a left parenthesis, parse it as '(' Expression ')'
            else if (_CurrentToken.Type == TokenType.LPRN)
            {
                _Eat(TokenType.LPRN);
                ASTNode tmpNode = _Expression();
                _Eat(TokenType.RPRN);
                return tmpNode;
            }

            else
            {
                return _Variable();
            }
        }

        private ASTTypeNode _Type()
        {
            if (_CurrentToken.Type == TokenType.INT || _CurrentToken.Type == TokenType.FLOAT)
            {
                Token tmp = _CurrentToken;
                _Eat(_CurrentToken.Type);
                return new ASTTypeNode(_CurrentToken);
            }

            else
                throw new Exception("Unrecognized type " + _CurrentToken.Value);
        }

        private List<ASTNode> _StatementList()
        {
            List<ASTNode> nodes = new List<ASTNode>();
            nodes.Add(_Statement());

            while (_CurrentToken.Type != TokenType.RCRLY)
            {
                nodes.Add(_Statement());
            }

            return nodes;
        }

        private ASTNode _Statement()
        {
            if (_CurrentToken.Type == TokenType.LCRLY)
            {
                return _CompoundStatement();
            }
            else if (_CurrentToken.Type == TokenType.ID)
            {
                return _AssignStatement();
            }
            else if (_CurrentToken.Type == TokenType.FLOAT ||
                _CurrentToken.Type == TokenType.INT)
            {
                return _DeclareStatement();
            }
            else if (_CurrentToken.Type == TokenType.RCRLY)
            {
                return _EmptyStatement();
            }
            else
            {
                throw new Exception("Unexpected token: " + _CurrentToken.Value);
            }
        }

        private ASTNode _DeclareStatement()
        {
            var type = _CurrentToken;

            if (_CurrentToken.Type == TokenType.INT
                || _CurrentToken.Type == TokenType.FLOAT)
            {
                _Eat(_CurrentToken.Type);
            }
            else
            {
                throw new Exception("Unexpected token: " + type.Value + ". Expected a type.");
            }

            //Parse the current token as a variable
            ASTVarNode name = _Variable();

            ASTDeclareNode node = new ASTDeclareNode(name,new ASTTypeNode(type));

            _Eat(TokenType.SEMI);
            return node;
            
            //TODO: Support assigning to a just-declared variable
        }
        
        private ASTAssignNode _AssignStatement()
        {
            var left = _Variable();
            _Eat(TokenType.ASN);
            var right = _Expression();
            _Eat(TokenType.SEMI);
            return new ASTAssignNode(left, right);
        }
        
        private ASTNoopNode _EmptyStatement()
        {
            return new ASTNoopNode();
        }
        
        private ASTVarNode _Variable()
        {
            var tmp = _CurrentToken;
            _Eat(TokenType.ID);

            return new ASTVarNode(tmp);
        }
    }
}
