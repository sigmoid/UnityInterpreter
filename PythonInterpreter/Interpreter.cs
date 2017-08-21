using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonInterpreter
{
    public class Interpreter
    {
        private AST _Tree;
        private Dictionary<string, object> GLOBAL_SCOPE;

        public Interpreter(AST tree)
        {
            _Tree = tree;
            GLOBAL_SCOPE = new Dictionary<string, object>();
        }

        public object Execute()
        {
            var i = _Visit(_Tree.Root);
            foreach (KeyValuePair<string, object> kvp in GLOBAL_SCOPE)
            {
                Console.WriteLine(kvp.Key + " = " + kvp.Value);
            }

            return i;
        }

        private object _Visit(ASTNode node)
        {
            if (node.GetType() == typeof(ASTBinopNode))
            {
                return _BinOpVisit(node);
            }
            else if (node.GetType() == typeof(ASTDeclareNode))
            {
                _VisitDeclare(node as ASTDeclareNode);
                return 0;
            }
            else if (node.GetType() == typeof(ASTUnaryNode))
            {
                return _UnOpVisit(node as ASTUnaryNode);
            }
            else if (node.GetType() == typeof(ASTCompoundNode))
            {
                _CompoundVisit(node as ASTCompoundNode);
                return 0;
            }
            else if (node.GetType() == typeof(ASTAssignNode))
            {
                _VisitAssign(node as ASTAssignNode);
                return 0;
            }
            else if (node.GetType() == typeof(ASTVarNode))
            {
                if (!GLOBAL_SCOPE.ContainsKey(node.Value.Value))
                    throw new KeyNotFoundException("Variable does not exist! " + node.Value.Value);
                else
                {
                    return GLOBAL_SCOPE[node.Value.Value];
                }

            }
            else if (node.GetType() == typeof(ASTNoopNode))
            {
                return 0;
            }
            else if (node.GetType() == typeof(ASTNumNode))
            {
                if(node.Value.Type == TokenType.INT_CONST)
                    return Int32.Parse(node.Value.Value);
                if (node.Value.Type == TokenType.FLOAT_CONST)
                    return float.Parse(node.Value.Value);
            }

            throw new Exception("Unexpected node type" + node.GetType());
        }

        private void _VisitDeclare(ASTDeclareNode node)
        {
            TokenType type = node.RightChild.Value.Type;
            string name = node.LeftChild.Value.Value;

            switch (type)
            {
                case TokenType.FLOAT:
                    GLOBAL_SCOPE.Add(name, (float)0);
                    return;
                case TokenType.INT:
                    GLOBAL_SCOPE.Add(name, (int)0);
                    return;

                default:
                    throw new Exception("Unknown type: " + type.ToString());
            }
        }

        private void _VisitAssign(ASTAssignNode node)
        {
            //If the variable is undefined
            if (!GLOBAL_SCOPE.ContainsKey(node.LeftChild.Value.Value))
            {
                GLOBAL_SCOPE.Add(node.LeftChild.Value.Value, 0);
            }

            GLOBAL_SCOPE[node.LeftChild.Value.Value] = _Visit(node.RightChild);
        }

        private void _CompoundVisit(ASTCompoundNode node)
        {
            foreach (ASTNode n in node.Children)
            {
                _Visit(n);
            }
        }

        private object _BinOpVisit(ASTNode node)
        {
            var left = _Visit(node.LeftChild);
            var right = _Visit(node.RightChild);

            if (left.GetType() == typeof(int) && right.GetType() == typeof(int)
                || left.GetType() == typeof(float) && right.GetType() == typeof(float))
            {
                float lhs = (float)left;
                float rhs = (float)right;

                if (node.Value.Type == TokenType.ADD)
                    return lhs + rhs;
                if (node.Value.Type == TokenType.SUB)
                    return lhs - rhs;
                if (node.Value.Type == TokenType.MUL)
                    return lhs * rhs;
                if (node.Value.Type == TokenType.DIV)
                    return lhs / rhs;
            }

            throw new Exception("Whdoido");
        }

        private object _UnOpVisit(ASTUnaryNode node)
        {
            var valu = _Visit(node.Expression);

            if (valu.GetType() == typeof(int) || valu.GetType() == typeof(float))
            {
                float val = (float)valu;
                if (node.Value.Type == TokenType.ADD)
                    return +val;
                if (node.Value.Type == TokenType.SUB)
                    return -val;
            }

            throw new Exception("Error parsing unary operator");
        }
    }
}
