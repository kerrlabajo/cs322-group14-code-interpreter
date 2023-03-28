using System;
using Antlr4.Runtime;
using Interpreter.Grammar;

namespace Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = System.IO.File.ReadAllText("test.txt");

            var inputStream = new AntlrInputStream(input);
            var lexer = new CodeGrammarLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new CodeGrammarParser(tokenStream);
            var tree = parser.program();

            var visitor = new CodeVisitor();
            visitor.Visit(tree);

            Console.ReadKey();
        }
    }
}
