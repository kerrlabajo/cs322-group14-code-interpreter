using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Interpreter.Helper;
using Interpreter.Model;

namespace Interpreter.Grammar
{
    public class CodeVisitor : CodeGrammarBaseVisitor<object?>
    {
        public Dictionary<string, object?> _variables { get; } = new Dictionary<string, object?>();

        public override object? VisitProgram([NotNull] CodeGrammarParser.ProgramContext context)
        {
            string program = context.GetText().Trim();
            if (program.StartsWith("BEGIN CODE") && program.EndsWith("END CODE"))
            {
                Console.WriteLine("Code contains BEGIN CODE and END CODE");
            }
            else if (program.StartsWith("BEGIN CODE") && program.EndsWith("BEGIN CODE"))
            {
                Console.WriteLine("Code is missing END CODE");
            }

            else if (program.EndsWith("END CODE"))
            {
                Console.WriteLine("Code is missing BEGIN CODE");
            }
            else
            {
                Console.WriteLine("Code does not contain BEGIN CODE and END CODE");
            }
            return null;
        }
    }
}
