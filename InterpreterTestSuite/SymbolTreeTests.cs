using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interpreter;

namespace InterpreterTestSuite
{
    [TestClass]
    public class SymbolTreeTests
    {
        [TestMethod]
        public void TestConstructor1()
        {
            TableTree tree = new TableTree();
        }

        [TestMethod]
        public void TestDefine1()
        {
            TableTree tree = new TableTree();
            tree.Define("Foobar", "string_Const");
            Assert.IsTrue(tree.Lookup("Foobar").Unassigned);
        }

        [TestMethod]
        public void TestLookup1()
        {
            TableTree tree = new TableTree();
            tree.Define("Foobar", "string_Const");
            tree.Bind("Foobar", "bar");
            Assert.IsFalse(tree.Lookup("Foobar").Unassigned);
            Assert.AreEqual(tree.Lookup("Foobar").GetValue(), "bar");
        }
    }
}
