using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            Lexer lex = new Lexer(File.Open("foobar.txt", FileMode.Open));
            Parser par = new Parser(lex);
            AST prog = par.Parse();
            var builder = new TableTreeBuilder();
            builder.Generate(prog);
            Interpreter inp = new Interpreter(prog);
            Console.WriteLine(inp.Execute());
        }
    }
}
