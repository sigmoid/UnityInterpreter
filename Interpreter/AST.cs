using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    /// <summary>
    /// Abstract Tree-based representation of a program to be interpreted or compiled.
    /// </summary>
    public class AST
    {
        public ASTNode Root;
        public AST(ASTNode root)
        {
            Root = root;
        }
    }

    public class ASTNode
    {
        public ASTNode LeftChild, RightChild;
        public Token Value;

        public ASTNode(Token value)
        {
            Value = value;
        }

        public ASTNode(ASTNode left, Token value, ASTNode right)
        {
            Value = value;
            LeftChild = left;
            RightChild = right;
        }

    }

    public class ASTNumNode : ASTNode
    {
        public ASTNumNode(Token val)
            : base(val)
        { }
    }

    public class ASTArgNode : ASTNode
    {
        public ASTArgNode(ASTTypeNode type)
            : base(new Token(TokenType.ARG, type.Value.Value))
        {
            LeftChild = type;    
        }
    }

    public class ASTFunctionDefNode : ASTNode
    {
        public List<ASTArgNode> Paramaters;
        public ASTCompoundNode Body;

        public ASTFunctionDefNode(string funcName)
            : base(new Token(TokenType.FUNCDEF,funcName))
        { }
    }

    public class ASTBinopNode : ASTNode
    {
        public ASTBinopNode(ASTNode left, Token t, ASTNode right)
            : base(left, t, right)
        { }
    }

    public class ASTUnaryNode : ASTNode
    {
        public ASTNode Expression;

        public ASTUnaryNode(Token Op, ASTNode Expr) : base(Op)
        {
            Expression = Expr;
        }
    }

    public class ASTCompoundNode : ASTNode
    {
        public List<ASTNode> Children;

        public ASTCompoundNode() :
            base(null)
        {
        }
    }

    public class ASTAssignNode : ASTNode
    {
        public ASTAssignNode(ASTNode left, ASTNode right)
            : base(left, new Token(TokenType.ASN, "="), right)
        {
        }
    }

    public class ASTVarNode : ASTNode
    {
        public ASTVarNode(Token val)
            : base(val)
        {
        }
    }

    public class ASTNoopNode : ASTNode
    {
        public ASTNoopNode()
            : base(null,null,null)
        {
        }
    }

    public class ASTDeclareNode : ASTNode
    {
        public ASTDeclareNode(ASTNode left, ASTNode right)
            : base(left, null, right)
        { 
        }
    }

    public class ASTTypeNode : ASTNode
    {
        public ASTTypeNode(Token tok)
            : base(tok)
        { }
    }
}
