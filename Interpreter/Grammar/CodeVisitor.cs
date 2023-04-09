using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

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
                foreach (KeyValuePair<string, object?> kvp in _variables)
                {
                    Console.WriteLine("Variable = {0}, Value = {1}", kvp.Key, kvp.Value);
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
            else if (context.singleAssignment() != null)
            {
                return VisitSingleAssignment(context.singleAssignment());
            }
            else if (context.multipleAssignments() != null)
            {
                return VisitMultipleAssignments(context.multipleAssignments());
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
            object? varValue = null;

            if (context.expression() != null)
            {
                varValue = Visit(context.expression());
            }

            foreach (var varName in varNames)
            {
                if (_variables.ContainsKey(varName))
                {
                    Console.WriteLine($"Variable '{varName}' is already defined!");
                    continue;
                }

                var convertedValue = varValue;

                if (varValue != null && type != varValue.GetType())
                {
                    if (TypeDescriptor.GetConverter(type).CanConvertFrom(varValue.GetType()))
                    {
                        convertedValue = TypeDescriptor.GetConverter(type).ConvertFrom(varValue);
                    }
                    else
                    {
                        Console.WriteLine($"Cannot convert value '{varValue}' to type '{typeStr}'");
                        continue;
                    }
                }

                _variables.Add(varName, convertedValue);
            }

            return null;
        }

        public override object? VisitVariable([NotNull] CodeGrammarParser.VariableContext context)
        {
            var dataTypeObj = VisitType(context.type());
            if (dataTypeObj is not Type dataType)
            {
                throw new ArgumentException("Invalid data type");
            }

            var variableName = context.IDENTIFIER().GetText();
            var variableValue = VisitExpression(context.expression());

            if (variableValue is null)
            {
                if (dataType.IsValueType)
                {
                    throw new ArgumentException("Cannot assign null to value type");
                }

                _variables[variableName] = null;
                return null;
            }

            if (dataType.IsAssignableFrom(variableValue.GetType()))
            {
                _variables[variableName] = variableValue;
                return variableValue;
            }

            try
            {
                var convertedValue = Convert.ChangeType(variableValue, dataType);
                _variables[variableName] = convertedValue;
                return convertedValue;
            }
            catch (Exception)
            {
                throw new ArgumentException($"Cannot convert value '{variableValue}' to type '{dataType.Name}'");
            }
        }

        public override object? VisitSingleAssignment([NotNull] CodeGrammarParser.SingleAssignmentContext context)
        {
            var variableName = context.IDENTIFIER().GetText();
            var variableValue = Visit(context.expression());

            _variables[variableName] = variableValue;

            return _variables[variableName];
        }

        public override object? VisitMultipleAssignments([NotNull] CodeGrammarParser.MultipleAssignmentsContext context)
        {
            var identifiers = context.IDENTIFIER();
            foreach (var identifier in identifiers)
            {
                string variableName = identifier.GetText();
                object? variableValue = context.expression().Accept(this);

                if (variableName == null || variableValue == null)
                {
                    throw new ArgumentNullException();
                }

                _variables[variableName] = variableValue;
            }
            return null;
        }

        public override object? VisitConstantValueExpression([NotNull] CodeGrammarParser.ConstantValueExpressionContext context)
        {
            string? constantValue = context.constant().GetText();
            bool isNegative = false;

            if (constantValue.StartsWith("-"))
            {
                isNegative = true;
                constantValue = constantValue.Substring(1);
            }

            switch (context.constant())
            {
                case var c when c.INTEGER_VALUES() != null:
                    int intValue = int.Parse(constantValue);
                    return isNegative ? -intValue : intValue;
                case var c when c.FLOAT_VALUES() != null:
                    float floatValue = float.Parse(constantValue);
                    return isNegative ? -floatValue : floatValue;
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

        public override object? VisitIdentifierExpression([NotNull] CodeGrammarParser.IdentifierExpressionContext context)
        {
            var variableName = context.IDENTIFIER().GetText();
            return _variables[variableName];
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
                foreach (char varChar in varName)
                {
                    if (varChar == '$')
                    {
                        Console.WriteLine();
                    }
                    else if (_variables.TryGetValue(varChar + "", out object? variableValue))
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

        public override object? VisitScan([NotNull] CodeGrammarParser.ScanContext context)
        {
            foreach (var id in context.IDENTIFIER().Select(x => x.GetText()).ToArray())
            {
                Console.Write($"Enter value for {id}: ");
                var input = Console.ReadLine();

                if (int.TryParse(input, out int intValue))
                {
                    _variables[id] = intValue;
                }
                else if (double.TryParse(input, out double doubleValue))
                {
                    _variables[id] = doubleValue;
                }
                else if (bool.TryParse(input, out bool boolValue))
                {
                    _variables[id] = boolValue;
                }
                else
                {
                    _variables[id] = input ?? "";
                }
            }

            return null;
        }

        public override object? VisitPositiveExpression(CodeGrammarParser.PositiveExpressionContext context)
        {
            return Visit(context.expression());
        }

        public override object? VisitNegativeExpression(CodeGrammarParser.NegativeExpressionContext context)
        {
            return -(double)Visit(context.expression());
        }
    }
}
