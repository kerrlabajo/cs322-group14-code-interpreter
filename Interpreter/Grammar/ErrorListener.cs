using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Interpreter.Grammar
{
    public class ErrorListener : BaseErrorListener
    {
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string error = $"line {line}:{charPositionInLine} {msg}";
            Console.Error.WriteLine(error);
            throw new ParseCanceledException(error);
        }
    }
}