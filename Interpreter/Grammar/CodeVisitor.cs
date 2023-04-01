using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;

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
                foreach (var linesContext in context.line())
                {
                    VisitLine(linesContext);
                }
                Console.WriteLine("Code is successful");
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

        public override object? VisitLine([NotNull] CodeGrammarParser.LineContext context)
        {
            if (context.initialization() != null)
            {
                return VisitInitialization(context.initialization());
            }
            else if (context.variable() != null)
            {
                return VisitVariable(context.variable());
            }
            else if (context.assignment() != null)
            {
                return VisitAssignment(context.assignment());
            }
            else if (context.ifBlock() != null)
            {
                return VisitIfBlock(context.ifBlock());
            }
            else if (context.whileBlock() != null)
            {
                return VisitWhileBlock(context.whileBlock());
            }
            else if (context.display() != null)
            {
                return VisitDisplay(context.display());
            }
            else if (context.scan() != null)
            {
                return VisitScan(context.scan());
            }
            else
            {
                throw new Exception("Syntax Error");
            }
        }

        public override object? VisitInitialization([NotNull] CodeGrammarParser.InitializationContext context)
        {
            // Map string type names to corresponding Type objects
            var typeMap = new Dictionary<string, Type>()
            {
                { "INT", typeof(int) },
                { "FLOAT", typeof(float) },
                { "BOOL", typeof(bool) },
                { "CHAR", typeof(char) },
                { "STRING", typeof(string) }
            };

            var typeStr = context.type().GetText();
            if (!typeMap.TryGetValue(typeStr, out var type))
            {
                Console.WriteLine($"Invalid variable type '{typeStr}'");
                return null;
            }

            var varNames = context.IDENTIFIER().Select(x => x.GetText()).ToArray();
            var varValue = Visit(context.expression());

            foreach (var varName in varNames)
            {
                if (_variables.ContainsKey(varName))
                {
                    Console.WriteLine($"Variable '{varName}' is already defined!");
                }
                else
                {
                    var convertedValue = varValue;
                    if (varValue != null && type != varValue.GetType())
                    {
                        convertedValue = TypeDescriptor.GetConverter(type).ConvertFrom(varValue);
                    }
                    _variables[varName] = convertedValue;
                }
            }

            return null;
        }

        public override object? VisitVariable([NotNull] CodeGrammarParser.VariableContext context)
        {
            var dataTypeObj = VisitType(context.type());
            if (dataTypeObj is null)
            {
                throw new Exception("Invalid data type");
            }

            var dataType = (Type)dataTypeObj;
            var variableName = context.IDENTIFIER().GetText();
            var variableValue = VisitExpression(context.expression());

            var varValueWithType = Convert.ChangeType(variableValue, dataType);
            _variables[variableName] = varValueWithType;

            return varValueWithType;
        }

        public override object? VisitAssignment([NotNull] CodeGrammarParser.AssignmentContext context)
        {
            var variableName = context.IDENTIFIER().GetText();
            var variableValue = Visit(context.expression());

            if (!_variables.ContainsKey(variableName))
            {
                Console.WriteLine($"Variable '{variableName}' is not defined!");
                return null;
            }

            var existingValue = _variables[variableName];
            if (existingValue == null && variableValue != null)
            {
                Console.WriteLine($"Cannot assign non-null value to null variable '{variableName}'");
                return null;
            }

            var existingType = existingValue?.GetType();
            var valueType = variableValue?.GetType();

            if (existingType != null && valueType != null && existingType != valueType)
            {
                Console.WriteLine($"Cannot assign value of type '{valueType.Name}' to variable '{variableName}' of type '{existingType.Name}'");
                return null;
            }

            _variables[variableName] = variableValue;
            return null;
        }

        public override object? VisitConstantValueExpression([NotNull] CodeGrammarParser.ConstantValueExpressionContext context)
        {
            string? constantValue = context.constant().GetText();
            switch (context.constant())
            {
                case var c when c.INTEGER_VALUES() != null:
                    return int.Parse(constantValue);
                case var c when c.FLOAT_VALUES() != null:
                    return float.Parse(constantValue);
                case var c when c.CHARACTER_VALUES() != null:
                    return constantValue[1];
                case var c when c.BOOLEAN_VALUES() != null:
                    return bool.Parse(constantValue.Trim('"').ToUpper());
                case var c when c.STRING_VALUES() != null:
                    return constantValue[1..^1];
                default:
                    return null;
            }
        }

        public override object? VisitType([NotNull] CodeGrammarParser.TypeContext context)
        {
            var dataTypeString = context.GetText();
            switch (dataTypeString.ToUpperInvariant())
            {
                case "INT":
                    return typeof(int);
                case "FLOAT":
                    return typeof(float);
                case "BOOL":
                    return typeof(bool);
                case "CHAR":
                    return typeof(char);
                case "STRING":
                    return typeof(string);
                default:
                    throw new Exception($"Invalid data type: {dataTypeString}");
            }
        }

        public override object? VisitDisplay([NotNull] CodeGrammarParser.DisplayContext context)
        {
            var varNamesToDisplay = context.expression().Select(x => x.GetText()).ToArray();
            foreach (var varName in varNamesToDisplay)
            {
                foreach(char varChar in varName)
                {
                    if(varChar == '$')
                    {
                        Console.WriteLine();
                    }
                    else if (_variables.TryGetValue(varChar+"", out object? variableValue))
                    {
                        if (variableValue is bool boolValue)
                        {
                            Console.Write(boolValue ? "TRUE" : "FALSE");
                        }
                        else if (variableValue is float floatValue)
                        {
                            Console.Write(floatValue.ToString("0.0###############"));
                        }
                        else
                        {
                            Console.Write(variableValue);
                        }
                    }
                }
            }

            Console.WriteLine();
            return null;
        }

    }
}
