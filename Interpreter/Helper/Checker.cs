﻿using Interpreter.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Helper
{
    public class Checker
    {
        public static bool CheckBeginAndEnd(CodeGrammarParser.BlockContext context)
        {
            string beginDelimiter = "BEGIN CODE";
            string endDelimiter = "END CODE";
            string? beginCode = context.BEGIN_CODE()?.GetText();
            string? endCode = context.END_CODE()?.GetText();
            string missingBeginCode = "<missing BEGIN_CODE>";
            string missingEndCode = "<missing END_CODE>";

            //Both delimiters are present in the code
            if (beginCode != null && endCode != null && beginCode != missingBeginCode && endCode != missingEndCode)
            {
                // Visit the declarations and lines only if the delimiters are at the beginning and end of the program
                if (context.declare().Length == 0 && context.line().Length == 0)
                {
                    Console.WriteLine("CODE not recognized \n" +
                                       "Cannot be executed");
                    Environment.Exit(1);
                }

                // Declaration contains BEGIN CODE
                if (context.declare().Length > 0 && context.declare().Any(d => d.GetText().Contains(beginDelimiter)))
                {
                    Console.WriteLine($"BEGIN CODE must only be at the beginning of the program");
                    Environment.Exit(1);
                }

                // Declaration contains END CODE
                if (context.declare().Length > 0 && context.declare().Any(d => d.GetText().Contains(endDelimiter) && d.GetText().IndexOf(endDelimiter) > 0))
                {
                    Console.WriteLine($"END CODE must only be at the end of the program");
                    Environment.Exit(1);
                }

                return true;
            }
            else if ((beginCode == null || beginCode == missingBeginCode) && (endCode != null || endCode != missingEndCode))
            {
                // Only the end delimiter is present
                Console.WriteLine("Missing BEGIN CODE");
                Environment.Exit(1);
            }

            else if ((beginCode != null || beginCode != missingBeginCode) && (endCode == null || endCode == missingEndCode))
            {
                // Only the begin delimiter is present
                Console.WriteLine("Missing END CODE");
                Environment.Exit(1);
            }

            else if ((beginCode == null || beginCode == missingBeginCode) && (endCode == null || endCode == missingEndCode))
            {
                // Neither delimiter is present
                Console.WriteLine("Missing BEGIN CODE and END CODE");
                Environment.Exit(1);
            }

            return false;
        }
    }
}