using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonInterpreter
{
    /// <summary>
    /// Utilizes a lexer to turn tokens into an AST for interpreting or compiling.
    /// This parser's grammar is a stripped-down version of C
    /// 
    /// Grammar
    /// ----------------------------------------------------------------------------------------
    /// Program             : Function_Definition
    /// Function_Definition : "function" ID LPAREN Paramaters RPAREN Compound_Statement
    /// Paramaters          : Paramater (, Paramater)*
    /// Paramater           : Type ID
    /// Compound_Statement  : LCRLY Statement_List RCRLY 
    /// Statement_List      : Statement (Statement_List)*
    /// Statement           : Compound_Statement | Assign_Statement | Declare_Statement | Empty_Statement
    /// Declare_Statement   : Type Variable SEMI | Type Assign_Statement
    /// Assign_Statement    : Variable ASSIGN expr SEMI
    /// Empty   `           :
    /// Expression          : Term ((ADD | SUB) Term)*
    /// Term                : Factor ((MUL | DIV) Factor)*
    /// Factor              : ADD Factor | SUB Factor | INT | LPAREN EXPR RPAREN | Variable
    /// Variable            : ID
    /// 
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

        public AST Parse()
        {
            return new AST(_Program());
        }

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


        /*
         * Grammar
        expr   : term((PLUS | MINUS) term)*
        term   : factor((MUL | DIV) factor)*
        factor : INTEGER | LPAREN expr RPAREN
        */
        private ASTNode _Expression()
        {
            var tmpNode = _Term();

            //If this token is either a plus or minus, add a binary operation to the tree
            while (_CurrentToken.Type == TokenType.ADD ||
                _CurrentToken.Type == TokenType.SUB)
            {
                var opToken = _CurrentToken;
                _Eat(_CurrentToken.Type);

                tmpNode = new ASTBinopNode(tmpNode, opToken, _Term());
            }

            return tmpNode;
        }

        /*
        term   : factor((MUL | DIV) factor)*
        factor : INTEGER | LPAREN expr RPAREN
        */
        private ASTNode _Term()
        {
            var tmpNode = _Factor();

            //If this token is either a plus or minus, add a binary operation to the tree
            while (_CurrentToken.Type == TokenType.MUL ||
                _CurrentToken.Type == TokenType.DIV)
            {
                var opToken = _CurrentToken;
                _Eat(_CurrentToken.Type);

                tmpNode = new ASTBinopNode(tmpNode, opToken, _Factor());
            }

            return tmpNode;
        }

        /*
        factor : (PLUS | MINUS) factor | INTEGER | LPAREN expr RPAREN
        */
        private ASTNode _Factor()
        {
            if (_CurrentToken.Type == TokenType.ADD || _CurrentToken.Type == TokenType.SUB)
            {
                var tmpToken = _CurrentToken;
                _Eat(_CurrentToken.Type);
                return new ASTUnaryNode(tmpToken, _Factor());
            }

            if (_CurrentToken.Type == TokenType.INT_CONST)
            {
                var tmpToken = _CurrentToken;
                _Eat(TokenType.INT_CONST);
                return new ASTNumNode(tmpToken);
            }
            if (_CurrentToken.Type == TokenType.FLOAT_CONST)
            {
                var tmpToken = _CurrentToken;
                _Eat(TokenType.FLOAT_CONST);
                return new ASTNumNode(tmpToken);
            }

            if (_CurrentToken.Type == TokenType.LPRN)
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
            throw new Exception("Unexpected Token " + _CurrentToken.Type);
        }

        private ASTNode _Program()
        {
            return _FunctionDefinition();
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

        private ASTFunctionDefNode _FunctionDefinition()
        {
            if(_CurrentToken.Value != "function")
            {
                throw new Exception("Token: " + _CurrentToken.Value + "not expected. Expected 'function'");
            }

            //Eat the "function" keyword
            _Eat(TokenType.ID);

            //Get the function name and set up the node
            string funcName = _CurrentToken.Value;
            _Eat(TokenType.ID);
            ASTFunctionDefNode ret = new ASTFunctionDefNode(funcName);
            ret.Value = new Token(TokenType.ID, funcName); 

            _Eat(TokenType.LPRN);


            ret.Paramaters = _Paramaters();

            _Eat(TokenType.RPRN);

            ret.Body = _CompoundStatement();

            return ret;
        }

        private List<ASTArgNode> _Paramaters()
        {
            List<ASTArgNode> nodes = new List<ASTArgNode>();
            ASTTypeNode type = _Type();
            nodes.Add(new ASTArgNode(type));

            //As long as there is a comma after this paramater
            while (_CurrentToken.Value == ",")
            {
                _Eat(TokenType.COMMA);
                type = _Type();
                nodes.Add(new ASTArgNode(type));
            }

            _Eat(TokenType.RPRN);

            return nodes;
        }

        private ASTTypeNode _Type()
        {
            if (_CurrentToken.Type == TokenType.INT || _CurrentToken.Type == TokenType.FLOAT)
            {
                Token tmp = _CurrentToken;
                _Eat(_CurrentToken.Type);
                _Eat(TokenType.ID);
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

        /// <summary>
        /// Statement for declaring a variable.
        /// Grammar
        /// Declare_Statement   : Type Variable SEMI !NOTYETSUPPORTED! | Type Assign_Statement !NOTYETSUPPORTED!
        /// </summary>
        /// <returns></returns>
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

            var name = _Variable();

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
