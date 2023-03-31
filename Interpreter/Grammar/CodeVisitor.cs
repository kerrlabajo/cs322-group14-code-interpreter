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
        // Variable name and its value
        private Dictionary<string, object?> _variable { get; } = new();
        // Variable name and its data type
        private Dictionary<string, string> _variableDeclarations { get; } = new Dictionary<string, string>();
        private bool _isBeginCodeVisited = false;
        private bool _isEndCodeVisited = false;

        public override object? VisitBlock([NotNull] CodeGrammarParser.BlockContext context)
        {
            if (Checker.CheckBeginAndEnd(context))
            {
                _isBeginCodeVisited = true;
                _isEndCodeVisited = true;
                return base.VisitBlock(context); // Visit the program normally
            }

            return null;
        }

        /*public override object VisitDeclare(CodeGrammarParser.DeclareContext context)
        {
           
        }*/
    }
}
