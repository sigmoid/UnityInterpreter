using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonInterpreter
{
    public class TableTree
    {
        private TableTreeNode _RootNode, _CurrentNode;

        public TableTree()
        {
            _RootNode = new TableTreeNode(null, new SymbolTable("GLOBAL"));
            _CurrentNode = _RootNode;
        }

        public Symbol Lookup(string symName)
        {
            return Lookup(symName, _CurrentNode);
        }

        public void Define(string name, string type)
        {
            _CurrentNode.Table.Define(name, type);
        }

        public void Bind(string name, string val)
        {
            _CurrentNode.Table.Bind(name, val);
        }
        private Symbol Lookup(string symName, TableTreeNode node)
        {
            var tmp = node.Table.Lookup(symName);

            if (tmp != null)
            {

                return tmp;
            }
            else
            {
                if (_CurrentNode.Parent == null)
                    return null;
                return Lookup(symName, _CurrentNode.Parent);
            }
        }

        public void AddTable(SymbolTable table)
        {
            _CurrentNode.AddChild(table);
        }

        public void EnterScope(string name)
        {
            _CurrentNode = _CurrentNode.Get(name);
        }

        public void ExitScope()
        {
            _CurrentNode = _CurrentNode.Parent;
        }

        internal class TableTreeNode
        {
            public TableTreeNode Parent;
            public Dictionary<string, TableTreeNode> Children;
            public SymbolTable Table;

            public TableTreeNode(TableTreeNode parent, SymbolTable table)
            {
                this.Parent = parent;
                this.Table = table;
                Children = new Dictionary<string, TableTreeNode>();
            }

            public TableTreeNode Get(string Name)
            {
                if (!Children.ContainsKey(Name))
                    return null;

                return Children[Name];
            }

            public void AddChild(SymbolTable table)
            {
                Children.Add(table.Name, new TableTreeNode(this, table));
            }
        }
    }

    public class TableTreeBuilder
    {
        private AST _AST;
        private TableTree _Tables;

        public TableTreeBuilder()
        {
        }

        public TableTree Generate(AST ast)
        {
            _AST = ast;
            _Tables = new TableTree();

            Visit(_AST.Root);

            return _Tables;
        }

        private void Visit(ASTNode node)
        {
            if (node == null)
                return;

            if (node.GetType() == typeof(ASTDeclareNode))
                _VisitDeclareNode(node as ASTDeclareNode);
            else if (node.GetType() == typeof(ASTVarNode))
                _VisitVarNode(node as ASTVarNode);
            else if (node.GetType() == typeof(ASTCompoundNode))
            {
                foreach (ASTNode n in (node as ASTCompoundNode).Children)
                    Visit(n);
            }
            else
            {
                Visit(node.LeftChild);
                Visit(node.RightChild);
            }
        }

        private void _VisitVarNode(ASTVarNode node)
        {
            if (_Tables.Lookup(node.Value.Value) == null)
            {
                throw new Exception("Variable " + node.Value.Value + " is undefined!");
            }
        }

        private void _VisitDeclareNode(ASTDeclareNode node)
        {
            if (_Tables.Lookup(node.LeftChild.Value.Value) != null)
            {
                throw new Exception("Variable " + node.LeftChild.Value.Value + " is defined more than once!");
            }

            _Tables.Define(node.LeftChild.Value.Value, node.RightChild.Value.Value);
        }
    }

    public class SymbolTable
    {
        public string Name;
        private Dictionary<string, Symbol> _Table;

        public SymbolTable(string name)
        {
            Name = name;
            _Table = new Dictionary<string, Symbol>();
        }

        public void Bind(string name, string val)
        {
            _Table[name].Assign(val);
        }

        public void Define(string name, string type)
        {
            var sym = new Symbol(null, type);
            _Table.Add(name, sym);
        }

        public Symbol Lookup(string name)
        {
            if (_Table.ContainsKey(name))
                return _Table[name];

            else
                return null;
        }
    }

    public class Symbol
    {
        private string _Val;
        public string Type;
        public bool Unassigned;

        public Symbol(string name, string type)
        {
            _Val = name;
            Type = type;
            Unassigned = true;
        }

        public void Assign(string val)
        {
            _Val = val;
            Unassigned = false;
        }

        public string GetValue()
        {
            return _Val;
        }
    }
}
