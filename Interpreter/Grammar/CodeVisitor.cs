﻿using System;
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
            object? varValue = null;
            if (context.expression() != null)
            {
                varValue = Visit(context.expression());
            }
            else if (context.ASSIGN() != null)
            {
                var defaultValueCtx = context.expression();
                if (defaultValueCtx != null)
                {
                    varValue = Convert.ChangeType(defaultValueCtx.GetText(), type);
                }
            }

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

                    // Only assign a value to the variable if it has a default value
                    if (varName == varNames.Last() && convertedValue != null)
                    {
                        _variables[varName] = convertedValue;
                    }
                    else
                    {
                        _variables[varName] = null;
                    }
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

            object? variableValue = null;
            if (context.expression() != null)
            {
                variableValue = VisitExpression(context.expression());
            }
            else if (_variables.ContainsKey(variableName))
            {
                variableValue = _variables[variableName];
            }

            object? varValueWithType = null;
            if (variableValue != null)
            {
                varValueWithType = Convert.ChangeType(variableValue, dataType);
            }

            _variables[variableName] = varValueWithType;

            return varValueWithType;
        }

        public override object? VisitAssignment([NotNull] CodeGrammarParser.AssignmentContext context)
        {
            object? variableValue = null;
            if (context.expression() != null)
            {
                variableValue = Visit(context.expression());
            }

            foreach (var childContext in context.children)
            {
                if (childContext is TerminalNodeImpl node && node.Symbol.Type == CodeGrammarLexer.IDENTIFIER)
                {
                    var variableName = node.GetText();

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
                }
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
            try
            {
                var varNamesToDisplay = context.expression().Select(x => x.GetText()).ToArray();
                foreach (var varName in varNamesToDisplay)
                {
                    char? prev = null;
                    foreach (char varChar in varName)
                    {
                        if (_variables.TryGetValue(varChar + "", out object? variableValue))
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
                        else if (varChar == '$' && prev != '[')
                        {
                            Console.WriteLine();
                        }
                        else if (varChar == '[' || varChar == ']' || varChar == '"')
                        {
                            prev = varChar;
                            continue;
                        }
                        else if (varChar != '&' || (varChar == '&' && prev == '['))
                        {
                            Console.Write(varChar);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

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
        public override object? VisitMultDivModExpression([NotNull] CodeGrammarParser.MultDivModExpressionContext context)
        {
            if (context.children.Count == 1)
            {
                // Return the single operator character as a string
                return context.GetText();
            }
            else
            {
                // There are two child nodes, so we need to handle the operator and the operands
                var left = Visit(context.expression(0));
                var right = Visit(context.expression(1));

                var op = context.highPrecedenceOperator().GetText();

                // Check the types of the operands
                if (left is int leftInt && right is int rightInt)
                {
                    // Both operands are integers
                    switch (op)
                    {
                        case "*":
                            return leftInt * rightInt;
                        case "/":
                            return leftInt / rightInt;
                        case "%":
                            return leftInt % rightInt;
                        default:
                            throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is float leftFloat && right is float rightFloat)
                {
                    // Both operands are doubles
                    switch (op)
                    {
                        case "*":
                            return leftFloat * rightFloat;
                        case "/":
                            return leftFloat / rightFloat;
                        case "%":
                            throw new ArgumentException($"Operator '%' cannot be applied to operands of type 'float'");
                        default:
                            throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is int leftInt2 && right is float rightFloat2)
                {
                    // One operand is an integer and the other is a double
                    switch (op)
                    {
                        case "*":
                            return leftInt2 * rightFloat2;
                        case "/":
                            return leftInt2 / rightFloat2;
                        case "%":
                            throw new ArgumentException($"Operator '%' cannot be applied to operands of type 'int' and 'float'");
                        default:
                            throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is float leftFloat2 && right is int rightInt2)
                {
                    // One operand is a double and the other is an integer
                    switch (op)
                    {
                        case "*":
                            return leftFloat2 * rightInt2;
                        case "/":
                            return leftFloat2 / rightInt2;
                        case "%":
                            throw new ArgumentException($"Operator '%' cannot be applied to operands of type 'float' and 'int'");
                        default:
                            throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left == null || right == null)
                {
                    throw new ArgumentNullException("Operand cannot be null.");
                }
                else
                {
                    // Operands are of different types
                    throw new ArgumentException($"Cannot perform operation on operands of different types: {left.GetType().Name} and {right.GetType().Name}");
                }
            }
        }

        public override object VisitAddSubConcatenatorExpression([NotNull] CodeGrammarParser.AddSubConcatenatorExpressionContext context)
        {
            // There are two child nodes, so we need to handle the operator and the operands
            var left = Visit(context.expression(0));
            var right = Visit(context.expression(1));

            var op = context.lowPrecedenceOperator().GetText();

            // Check the types of the operands
            if (left is int leftInt && right is int rightInt)
            {
                // Both operands are integers
                if (op == "+")
                {
                    return leftInt + rightInt;
                }
                else if (op == "-")
                {
                    return leftInt - rightInt;
                }
                else if (op == "&")
                {
                    return leftInt.ToString() + rightInt.ToString();
                }
                else
                {
                    throw new ArgumentException($"Unknown operator: {op}");
                }
            }
            else if (left is float leftFloat && right is float rightFloat)
            {
                // Both operands are floats
                if (op == "+")
                {
                    return leftFloat + rightFloat;
                }
                else if (op == "-")
                {
                    return leftFloat - rightFloat;
                }
                else if (op == "&")
                {
                    return leftFloat.ToString() + rightFloat.ToString();
                }
                else
                {
                    throw new ArgumentException($"Unknown operator: {op}");
                }
            }
            else if (left is int leftInt2 && right is float rightFloat2)
            {
                // One operand is an integer and the other is a float
                if (op == "+")
                {
                    return leftInt2 + rightFloat2;
                }
                else if (op == "-")
                {
                    return leftInt2 - rightFloat2;
                }
                else if (op == "&")
                {
                    return leftInt2.ToString() + rightFloat2.ToString();
                }
                else
                {
                    throw new ArgumentException($"Unknown operator: {op}");
                }
            }
            else if (left is float leftFloat2 && right is int rightInt2)
            {
                // One operand is a float and the other is an integer
                if (op == "+")
                {
                    return leftFloat2 + rightInt2;
                }
                else if (op == "-")
                {
                    return leftFloat2 - rightInt2;
                }
                else if (op == "&")
                {
                    return leftFloat2.ToString() + rightInt2.ToString();
                }
                else
                {
                    throw new ArgumentException($"Unknown operator: {op}");
                }
            }
            else if (left == null || right == null)
            {
                throw new ArgumentNullException("Operand cannot be null.");
            }
            else
            {
                // Operands are of different types
                throw new ArgumentException($"Cannot perform operation on operands of different types: {left.GetType().Name} and {right.GetType().Name}");
            }
        }
    }
}
