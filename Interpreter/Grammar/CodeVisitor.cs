using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Interpreter.Grammar
{
    public class CodeVisitor : CodeGrammarBaseVisitor<object?>
    {
        public Dictionary<string, object?> _variables { get; } = new Dictionary<string, object?>();
        public Dictionary<string, string?> _varTypes { get; } = new Dictionary<string, string?>();

        public override object? VisitInitialization([NotNull] CodeGrammarParser.InitializationContext context)
        {
            try
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
                    throw new ArgumentException($"Invalid variable type '{typeStr}'");
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
                        throw new ArgumentException($"Variable '{varName}' is already defined!");
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
                            throw new ArgumentException($"Cannot convert value '{varValue}' to type '{typeStr}'");
                        }
                    }

                    _variables.Add(varName, convertedValue);
                    _varTypes.Add(varName, typeStr);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught: {ex.Message}");
                return null;
            }
        }


        public override object? VisitVariable([NotNull] CodeGrammarParser.VariableContext context)
        {
            try
            {
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
                    throw new ArgumentException($"Invalid variable type '{typeStr}'");
                }

                // Loop over all the identifiers and add them to the dictionary
                for (int i = 0; i < context.IDENTIFIER().Length; i++)
                {
                    var identifier = context.IDENTIFIER(i).GetText();
                    var expression = context.expression(i) != null ? Visit(context.expression(i)) : null;
                    var variable = expression != null ? expression : null;

                    _variables[identifier] = variable;
                    _varTypes[identifier] = typeStr;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught: {ex.Message}");
                return null;
            }
        }



        public override object? VisitSingleAssignment([NotNull] CodeGrammarParser.SingleAssignmentContext context)
        {
            try
            {
                var variableName = context.IDENTIFIER().GetText();
                var variableValue = Visit(context.expression());

                _variables[variableName] = variableValue;

                return _variables[variableName];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught: {ex.Message}");
                return null;
            }
        }


        public override object? VisitMultipleAssignments([NotNull] CodeGrammarParser.MultipleAssignmentsContext context)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught: {ex.Message}");
                return null;
            }
        }


        public override object? VisitConstantValueExpression([NotNull] CodeGrammarParser.ConstantValueExpressionContext context)
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine($"Error in parsing constant value: {e.Message}");
                return null;
            }
        }


        public override object? VisitIdentifierExpression([NotNull] CodeGrammarParser.IdentifierExpressionContext context)
        {
            try
            {
                var variableName = context.IDENTIFIER().GetText();
                return _variables[variableName];
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine($"Variable '{context.IDENTIFIER().GetText()}' has not been declared.");
                return null;
            }
        }


        public override object? VisitType([NotNull] CodeGrammarParser.TypeContext context)
        {
            try
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
            catch (Exception ex)
            {
               Console.WriteLine($"Error in VisitType method: {ex.Message}");
                return null;
            }
        }


        public override object? VisitDisplay([NotNull] CodeGrammarParser.DisplayContext context)
        {
            try
            {
                var varNamesToDisplay = context.expression().Select(Visit).ToArray();
                var varValues = string.Join("", varNamesToDisplay.Select(var => var?.ToString()));
                varValues = varValues.Replace("True", "TRUE");
                varValues = varValues.Replace("False", "FALSE");

                Console.Write(varValues);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred while displaying expression: {e.Message}");
            }
            return null;
        }


        public override object? VisitScan([NotNull] CodeGrammarParser.ScanContext context)
        {
            try
            {
                foreach (var id in context.IDENTIFIER().Select(x => x.GetText()).ToArray())
                {
                    var input = Console.ReadLine();

                    if (_varTypes[id] == "INT")
                    {
                        if (int.TryParse(input, out int intValue))
                        {
                            _variables[id] = intValue;
                        }
                        else
                        {
                            throw new InvalidCastException($"The inputted value is not of type {_varTypes[id]}.");
                        }
                    }
                    else if (_varTypes[id] == "FLOAT")
                    {
                        if (float.TryParse(input, out float floatValue))
                        {
                            _variables[id] = floatValue;
                        }
                        else
                        {
                            throw new InvalidCastException($"The inputted value is not of type {_varTypes[id]}.");
                        }
                    }
                    else if (_varTypes[id] == "BOOL")
                    {
                        if (bool.TryParse(input, out bool boolValue))
                        {
                            _variables[id] = boolValue;
                        }
                        else
                        {
                            throw new InvalidCastException($"The inputted value is not of type {_varTypes[id]}.");
                        }
                    }
                    else if (_varTypes[id] == "CHAR")
                    {
                        if (char.TryParse(input, out char charValue))
                        {
                            _variables[id] = charValue;
                        }
                        else
                        {
                            throw new InvalidCastException($"The inputted value is not of type {_varTypes[id]}.");
                        }
                    }
                    else if (_varTypes[id] == "STRING")
                    {
                        _variables[id] = input ?? "";
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }


        public override object? VisitNegativeExpression(CodeGrammarParser.NegativeExpressionContext context)
        {
            try
            {
#pragma warning disable CS8605 // Unboxing a possibly null value.
                return -(double)Visit(context.expression());
#pragma warning restore CS8605 // Unboxing a possibly null value.
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception caught: {e.Message}");
                return null;
            }
        }

        public override object? VisitNextLineExpression([NotNull] CodeGrammarParser.NextLineExpressionContext context)
        {
            try
            {
                return "\n";
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public override object? VisitConcatExpression([NotNull] CodeGrammarParser.ConcatExpressionContext context)
        {
            try
            {
                var left = Visit(context.expression(0));
                var right = Visit(context.expression(1));

                return $"{left}{right}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public override object? VisitParenthesisExpression([NotNull] CodeGrammarParser.ParenthesisExpressionContext context)
        {
            try
            {
                // Visit the inner expression and return its result
                return Visit(context.expression());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }


        public override object? VisitMultDivModExpression([NotNull] CodeGrammarParser.MultDivModExpressionContext context)
        {
            try
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
                            case "&":
                                return leftInt.ToString() + rightInt.ToString();
                            case "$":
                                return leftInt.ToString() + "\n" + rightInt.ToString();
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
                            case "&":
                                return leftFloat.ToString("0.0###############") + rightFloat.ToString("0.0###############");
                            case "$":
                                return leftFloat.ToString("0.0###############") + "\n" + rightFloat.ToString("0.0###############");
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
                            case "&":
                                return leftInt2.ToString() + rightFloat2.ToString("0.0###############");
                            case "$":
                                return leftInt2.ToString() + "\n" + rightFloat2.ToString("0.0###############");
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
                            case "&":
                                return leftFloat2.ToString("0.0###############") + rightInt2.ToString();
                            case "$":
                                return leftFloat2.ToString("0.0###############") + "\n" + rightInt2.ToString();
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
                    else if (left is char leftChar3 && right is float rightFloat4)
                    {
                        if (op == "&")
                        {
                            return leftChar3.ToString() + rightFloat4.ToString("0.0###############");
                        }
                        else if (op == "$")
                        {
                            return leftChar3.ToString() + "\n" + rightFloat4.ToString("0.0###############");
                        }
                        else
                        {
                            Console.WriteLine("leftChar3 + rightFloat4");
                            throw new ArgumentException($"Unknown operator: {op}");
                        }
                    }
                    else if (left is float leftFloat4 && right is char rightChar3)
                    {
                        if (op == "&")
                        {
                            return leftFloat4.ToString("0.0###############") + rightChar3;
                        }
                        else if (op == "$")
                        {
                            return leftFloat4.ToString("0.0###############") + "\n" + rightChar3;
                        }
                        else
                        {
                            Console.WriteLine("leftFloat4 + rightChar3");
                            throw new ArgumentException($"Unknown operator: {op}");
                        }
                    }
                    else if (left is string leftString && right is string rightString)
                    {
                        if (op == "&")
                        {
                            return leftString + rightString;
                        }
                        else if (op == "$")
                        {
                            return leftString + "\n" + rightString;
                        }
                        else
                        {
                            Console.WriteLine("leftString + rightString");
                            throw new ArgumentException($"Unknown operator: {op}");
                        }
                    }
                    else if (left is string leftString1 && right is int rightInt5)
                    {
                        if (op == "&")
                        {
                            return leftString1 + rightInt5;
                        }
                        else if (op == "$")
                        {
                            return leftString1 + "\n" + rightInt5;
                        }
                        else if (op == "*")
                        {
                            //Add these similarly in MultiDivModExpresison chuchu
                            string numberString = new string(leftString1.Where(char.IsDigit).ToArray());
                            if (int.TryParse(numberString, out int leftNumber))
                            {
                                int result = leftNumber * rightInt5;
                                return new string(leftString1.Where(c => char.IsLetter(c) || char.IsSymbol(c) || char.IsPunctuation(c) || c == '#' || c == '$' || c == '&').ToArray()) + result.ToString();

                            }
                        }
                        else if (op == "/")
                        {
                            string numberString = new string(leftString1.Where(char.IsDigit).ToArray());
                            if (int.TryParse(numberString, out int leftNumber))
                            {
                                int result = leftNumber / rightInt5;
                                return new string(leftString1.Where(c => char.IsLetter(c) || char.IsSymbol(c) || char.IsPunctuation(c) || c == '#' || c == '$' || c == '&').ToArray()) + result.ToString();
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"Unknown operator: {op}");
                        }
                        //Until here
                    }

                    else if (left is int leftInt5 && right is string rightString1)
                    {
                        if (op == "&")
                        {
                            return leftInt5 + rightString1;
                        }
                        else if (op == "$")
                        {
                            return leftInt5 + "\n" + rightString1;
                        }
                        else if (op == "*")
                        {
                            string numberString = new string(rightString1.Where(char.IsDigit).ToArray());
                            if (int.TryParse(numberString, out int rightNumber))
                            {
                                int result = leftInt5 * rightNumber;
                                return new string(rightString1.Where(c => char.IsLetter(c) || char.IsSymbol(c) || char.IsPunctuation(c) || c == '#' || c == '$' || c == '&').ToArray()) + result.ToString();

                            }
                        }
                        else if (op == "/")
                        {
                            string numberString = new string(rightString1.Where(char.IsDigit).ToArray());
                            if (int.TryParse(numberString, out int rightNumber))
                            {
                                int result = leftInt5 / rightNumber;
                                return new string(rightString1.Where(c => char.IsLetter(c) || char.IsSymbol(c) || char.IsPunctuation(c) || c == '#' || c == '$' || c == '&').ToArray()) + result.ToString();
                            }
                        }
                        else
                        {
                            Console.WriteLine("leftInt5 * rightString1");
                            throw new ArgumentException($"Unknown operator: {op}");
                        }
                    }
                    else
                    {
                        // Operands are of different types
                        throw new ArgumentException($"Cannot perform operation on operands of different types: {left.GetType().Name} and {right.GetType().Name}");
                    }
                }

                }catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        public override object? VisitAddSubConcatenatorExpression([NotNull] CodeGrammarParser.AddSubConcatenatorExpressionContext context)
        {
            // There are two child nodes, so we need to handle the operator and the operands
            try
            {
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
                    else if (op == "$")
                    {
                        return leftInt.ToString() + "\n" + rightInt.ToString();
                    }
                    else
                    {
                        Console.WriteLine("leftint + rightint");
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
                        return leftFloat.ToString("0.0###############") + rightFloat.ToString("0.0###############");
                    }
                    else if (op == "$")
                    {
                        return leftFloat.ToString("0.0###############") + "\n" + rightFloat.ToString("0.0###############");
                    }
                    else
                    {
                        Console.WriteLine("leftFloat + rightFloat");
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
                        return leftInt2.ToString() + rightFloat2.ToString("0.0###############");
                    }
                    else if (op == "$")
                    {
                        return leftInt2.ToString() + "\n" + rightFloat2.ToString("0.0###############");
                    }
                    else
                    {
                        Console.WriteLine("leftFloat + rightFloat");
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
                        return leftFloat2.ToString("0.0###############") + rightInt2.ToString();
                    }
                    else
                    {
                        Console.WriteLine("leftFloat + rightFloat");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is bool leftBool && right is bool rightBool)
                {
                    if (op == "&")
                    {
                        return leftBool + "" + rightBool;
                    }
                    else if (op == "$")
                    {
                        return leftBool + "\n" + rightBool;
                    }
                    else
                    {
                        Console.WriteLine("leftBool + rightBool");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is bool leftBool1 && right is int rightInt3)
                {
                    if (op == "&")
                    {
                        return leftBool1 + rightInt3.ToString();
                    }
                    else if (op == "$")
                    {
                        return leftBool1 + "\n" + rightInt3.ToString();
                    }
                    else
                    {
                        Console.WriteLine("leftBool + rightInt3");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is int leftInt3 && right is bool rightBool1)
                {
                    if (op == "&")
                    {
                        return leftInt3.ToString() + rightBool1;
                    }
                    else if (op == "$")
                    {
                        return leftInt3.ToString() + "\n" + rightBool1;
                    }
                    else
                    {
                        Console.WriteLine("leftInt3 + rightBool1");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is bool leftBool2 && right is float rightFloat3)
                {
                    if (op == "&")
                    {
                        return leftBool2 + rightFloat3.ToString("0.0###############");
                    }
                    else if (op == "$")
                    {
                        return leftBool2 + "\n" + rightFloat3.ToString("0.0###############");
                    }
                    else
                    {
                        Console.WriteLine("leftBool2 + rightFloat3");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is float leftFloat3 && right is bool rightBool2)
                {
                    if (op == "&")
                    {
                        return leftFloat3.ToString("0.0###############") + rightBool2;
                    }
                    else if (op == "$")
                    {
                        return leftFloat3.ToString("0.0###############") + "\n" + rightBool2;
                    }
                    else
                    {
                        Console.WriteLine("leftFloat3 + rightBool2");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is bool leftBool3 && right is char rightChar)
                {
                    if (op == "&")
                    {
                        return leftBool3 + "" + rightChar;
                    }
                    else if (op == "$")
                    {
                        return leftBool3 + "\n" + rightChar;
                    }
                    else
                    {
                        Console.WriteLine("leftBool3 + rightChar");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is char leftChar && right is bool rightBool3)
                {
                    if (op == "&")
                    {
                        return leftChar + "" + rightBool3;
                    }
                    else if (op == "$")
                    {
                        return leftChar + "\n" + rightBool3;
                    }
                    else
                    {
                        Console.WriteLine("leftChar + rightBool3");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is char leftChar1 && right is char rightChar1)
                {
                    if (op == "&")
                    {
                        return leftChar1.ToString() + rightChar1;
                    }
                    else if (op == "$")
                    {
                        return leftChar1.ToString() + "\n" + rightChar1;
                    }
                    else
                    {
                        Console.WriteLine("leftChar + rightChar1");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is char leftChar2 && right is int rightInt4)
                {
                    if (op == "&")
                    {
                        return leftChar2 + rightInt4.ToString();
                    }
                    else if (op == "$")
                    {
                        return leftChar2 + "\n" + rightInt4.ToString();
                    }
                    else
                    {
                        Console.WriteLine("leftChar + rightInt4");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is int leftInt4 && right is char rightChar2)
                {
                    if (op == "&")
                    {
                        return leftInt4.ToString() + rightChar2;
                    }
                    else if (op == "$")
                    {
                        return leftInt4.ToString() + "\n" + rightChar2;
                    }
                    else
                    {
                        Console.WriteLine("leftInt4 + rightChar2");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is char leftChar3 && right is float rightFloat4)
                {
                    if (op == "&")
                    {
                        return leftChar3.ToString() + rightFloat4.ToString("0.0###############");
                    }
                    else if (op == "$")
                    {
                        return leftChar3.ToString() + "\n" + rightFloat4.ToString("0.0###############");
                    }
                    else
                    {
                        Console.WriteLine("leftChar3 + rightFloat4");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is float leftFloat4 && right is char rightChar3)
                {
                    if (op == "&")
                    {
                        return leftFloat4.ToString("0.0###############") + rightChar3;
                    }
                    else if (op == "$")
                    {
                        return leftFloat4.ToString("0.0###############") + "\n" + rightChar3;
                    }
                    else
                    {
                        Console.WriteLine("leftFloat4 + rightChar3");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is string leftString && right is string rightString)
                {
                    if (op == "&")
                    {
                        return leftString + rightString;
                    }
                    else if (op == "$")
                    {
                        return leftString + "\n" + rightString;
                    }
                    else
                    {
                        Console.WriteLine("leftString + rightString");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is string leftString1 && right is int rightInt5)
                {
                    if (op == "&")
                    {
                        return leftString1 + rightInt5;
                    }
                    else if (op == "$")
                    {
                        return leftString1 + "\n" + rightInt5;
                    }
                    else if (op == "+")
                    {
                        //Add these similarly in MultiDivModExpresison chuchu
                        string numberString = new string(leftString1.Where(char.IsDigit).ToArray());
                        if (int.TryParse(numberString, out int leftNumber))
                        {
                            int result = leftNumber + rightInt5;
                            return new string(leftString1.Where(c => char.IsLetter(c) || char.IsSymbol(c) || char.IsPunctuation(c) || c == '#' || c == '$' || c == '&').ToArray()) + result.ToString();

                        }
                        else
                        {
                            return leftString1 + rightInt5;
                        }
                    }
                    else if (op == "-")
                    {
                        string numberString = new string(leftString1.Where(char.IsDigit).ToArray());
                        if (int.TryParse(numberString, out int leftNumber))
                        {
                            int result = leftNumber - rightInt5;
                            return new string(leftString1.Where(c => char.IsLetter(c) || char.IsSymbol(c) || char.IsPunctuation(c) || c == '#' || c == '$' || c == '&').ToArray()) + result.ToString();
                        }
                        else
                        {
                            return leftString1 + rightInt5;
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                    //Until here
                }

                else if (left is int leftInt5 && right is string rightString1)
                {
                    if (op == "&")
                    {
                        return leftInt5 + rightString1;
                    }
                    else if (op == "$")
                    {
                        return leftInt5 + "\n" + rightString1;
                    }
                    else if (op == "+")
                    {
                        string numberString = new string(rightString1.Where(char.IsDigit).ToArray());
                        if (int.TryParse(numberString, out int rightNumber))
                        {
                            int result = leftInt5 + rightNumber;
                            return new string(rightString1.Where(c => char.IsLetter(c) || char.IsSymbol(c) || char.IsPunctuation(c) || c == '#' || c == '$' || c == '&').ToArray()) + result.ToString();

                        }
                        else
                        {
                            return leftInt5 + rightString1;
                        }
                    }
                    else if (op == "-")
                    {
                        string numberString = new string(rightString1.Where(char.IsDigit).ToArray());
                        if (int.TryParse(numberString, out int rightNumber))
                        {
                            int result = leftInt5 - rightNumber;
                            return new string(rightString1.Where(c => char.IsLetter(c) || char.IsSymbol(c) || char.IsPunctuation(c) || c == '#' || c == '$' || c == '&').ToArray()) + result.ToString();
                        }
                        else
                        {
                            return rightString1 + rightNumber;
                        }
                    }
                    else
                    {
                        Console.WriteLine("leftInt5 + rightString1");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is string leftString2 && right is float rightFloat5)
                {
                    if (op == "&")
                    {
                        return leftString2 + rightFloat5.ToString("0.0###############");
                    }
                    else if (op == "$")
                    {
                        return leftString2 + "\n" + rightFloat5.ToString("0.0###############");
                    }
                    else
                    {
                        Console.WriteLine("leftString2 + rightFloat5");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is float leftFloat5 && right is string rightString2)
                {
                    if (op == "&")
                    {
                        return leftFloat5.ToString("0.0###############") + rightString2;
                    }
                    else if (op == "$")
                    {
                        return leftFloat5.ToString("0.0###############") + "\n" + rightString2;
                    }
                    else
                    {
                        Console.WriteLine("leftFloat5 + rightString2");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is string leftString3 && right is char rightChar4)
                {
                    if (op == "&")
                    {
                        return leftString3 + rightChar4;
                    }
                    else if (op == "$")
                    {
                        return leftString3 + "\n" + rightChar4;
                    }
                    else
                    {
                        Console.WriteLine("leftString3 + rightChar4");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is char leftChar4 && right is string rightString3)
                {
                    if (op == "&")
                    {
                        return leftChar4 + rightString3;
                    }
                    else if (op == "$")
                    {
                        return leftChar4 + "\n" + rightString3;
                    }
                    else
                    {
                        Console.WriteLine("leftChar4 + rightString3");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is string leftString4 && right is bool rightBool4)
                {
                    if (op == "&")
                    {
                        return leftString4 + rightBool4;
                    }
                    else if (op == "$")
                    {
                        return leftString4 + "\n" + rightBool4;
                    }
                    else
                    {
                        Console.WriteLine("leftString4 + rightBool4");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is bool leftBool4 && right is string rightString4)
                {
                    if (op == "&")
                    {
                        return leftBool4 + rightString4;
                    }
                    else if (op == "$")
                    {
                        return leftBool4 + "\n" + rightString4;
                    }
                    else
                    {
                        Console.WriteLine("leftBool4 + rightString4");
                        throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (right == null && op == "$")
                {
                    switch (left)
                    {
                        case int leftInt6:
                            return leftInt6.ToString() + "\n";
                        case float leftFloat6:
                            return leftFloat6.ToString("0.0###############") + "\n";
                        case char leftChar6:
                            return leftChar6 + "\n";
                        case string leftString5:
                            return leftString5.ToString() + "\n";
                        case bool leftBool5:
                            return leftBool5 + "\n";
                        default:
                            break;
                    }
                    Console.WriteLine("rightNull + op$");
                    return null;
                }
                else if (left == null && op == "$")
                {
                    switch (right)
                    {
                        case int rightInt6:
                            return rightInt6.ToString() + "\n";
                        case float rightFloat6:
                            return rightFloat6.ToString("0.0###############") + "\n";
                        case char rightChar6:
                            return rightChar6 + "\n";
                        case string rightString5:
                            return rightString5.ToString() + "\n";
                        case bool rightBool5:
                            return rightBool5 + "\n";
                        default:
                            break;
                    }
                    Console.WriteLine("leftNull + op$");
                    return null;
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
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public override object? VisitComparisonExpression(CodeGrammarParser.ComparisonExpressionContext context)
        {
            try
            {
                var left = Visit(context.expression(0));
                var right = Visit(context.expression(1));
                var op = context.comparisonOperator().GetText();

                // Check the types of the operands
                if (left is int leftInt && right is int rightInt)
                {
                    // Both operands are integers
                    switch (op)
                    {
                        case "==":
                            return leftInt == rightInt;
                        case "<>":
                            return leftInt != rightInt;
                        case ">":
                            return leftInt > rightInt;
                        case "<":
                            return leftInt < rightInt;
                        case ">=":
                            return leftInt >= rightInt;
                        case "<=":
                            return leftInt <= rightInt;
                        default:
                            throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is float leftFloat && right is float rightFloat)
                {
                    // Both operands are floats
                    switch (op)
                    {
                        case "==":
                            return leftFloat == rightFloat;
                        case "<>":
                            return leftFloat != rightFloat;
                        case ">":
                            return leftFloat > rightFloat;
                        case "<":
                            return leftFloat < rightFloat;
                        case ">=":
                            return leftFloat >= rightFloat;
                        case "<=":
                            return leftFloat <= rightFloat;
                        default:
                            throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is int leftInt2 && right is float rightFloat2)
                {
                    // One operand is an integer and the other is a float
                    switch (op)
                    {
                        case "==":
                            return leftInt2 == rightFloat2;
                        case "<>":
                            return leftInt2 != rightFloat2;
                        case ">":
                            return leftInt2 > rightFloat2;
                        case "<":
                            return leftInt2 < rightFloat2;
                        case ">=":
                            return leftInt2 >= rightFloat2;
                        case "<=":
                            return leftInt2 <= rightFloat2;
                        default:
                            throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is float leftFloat2 && right is int rightInt2)
                {
                    // One operand is a float and the other is an integer
                    switch (op)
                    {
                        case "==":
                            return leftFloat2 == rightInt2;
                        case "<>":
                            return leftFloat2 != rightInt2;
                        case ">":
                            return leftFloat2 > rightInt2;
                        case "<":
                            return leftFloat2 < rightInt2;
                        case ">=":
                            return leftFloat2 >= rightInt2;
                        case "<=":
                            return leftFloat2 <= rightInt2;
                        default:
                            throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is bool leftBool && right is bool rightBool)
                {
                    // Both operands are booleans
                    switch (op)
                    {
                        case "==":
                            return leftBool == rightBool;
                        case "<>":
                            return leftBool != rightBool;
                        default:
                            throw new ArgumentException($"Unknown operator: {op}");
                    }
                }
                else if (left is char leftChar && right is char rightChar)
                {
                    switch (op)
                    {
                        case "==":
                            return leftChar == rightChar;
                        case "<>":
                            return leftChar != rightChar;
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
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public override object? VisitLogicalExpression([NotNull] CodeGrammarParser.LogicalExpressionContext context)
        {
            try
            {
                var left = Visit(context.expression(0));
                var right = Visit(context.expression(1));
                var op = context.logicalOperator().GetText();

                // Check the types of the operands
                if (left is bool leftBool && right is bool rightBool)
                {
                    // Both operands are booleans
                    switch (op)
                    {
                        case "AND":
                            return leftBool && rightBool;
                        case "OR":
                            return leftBool || rightBool;
                        case "NOT":
                            return !leftBool;
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
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public override object? VisitIfBlock([NotNull] CodeGrammarParser.IfBlockContext context)
        {
            try
            {
                // Evaluate the expression inside the if statement
#pragma warning disable CS8605 // Unboxing a possibly null value.
                bool condition = (bool)Visit(context.expression());
#pragma warning restore CS8605 // Unboxing a possibly null value.

                // If the condition is true, execute the code inside the if block
                if (condition)
                {
                    // Visit all the lines of code inside the if block
                    foreach (var lineContext in context.line())
                    {
                        Visit(lineContext);
                    }
                }
                // If there's an else if block, evaluate its condition and execute its code if it's true
                else if (context.elseIfBlock() != null)
                {
                    return Visit(context.elseIfBlock());
                }
                // If there's an else block, execute its code 
                else if (context.elseBlock() != null)
                {
                    Visit(context.elseBlock());
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public override object? VisitElseIfBlock([NotNull] CodeGrammarParser.ElseIfBlockContext context)
        {
            try
            {
                // Evaluate the condition of the else if block
#pragma warning disable CS8605 // Unboxing a possibly null value.
                bool condition = (bool)Visit(context.expression());
#pragma warning restore CS8605 // Unboxing a possibly null value.

                // If the condition is true, execute the code inside the else if block
                if (condition)
                {
                    // Visit all the lines of code inside the else if block
                    foreach (var lineContext in context.line())
                    {
                        Visit(lineContext);
                    }
                }
                // If there's another else if block, evaluate its condition and execute its code if it's true
                else if (context.elseIfBlock() != null)
                {
                    return Visit(context.elseIfBlock());
                }
                // If there's an else block, execute its code
                else if (context.elseBlock() != null)
                {
                    Visit(context.elseBlock());
                }
            }catch(Exception ex) { 
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public override object? VisitElseBlock([NotNull] CodeGrammarParser.ElseBlockContext context)
        {
            try
            {
                // Visit all the lines of code inside the else block
                foreach (var lineContext in context.line())
                {
                    Visit(lineContext);
                }
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public override object? VisitWhileBlock([NotNull] CodeGrammarParser.WhileBlockContext context)
        {
            try
            {
                // Get the expression from the context
#pragma warning disable CS8605 // Unboxing a possibly null value.
                bool condition = (bool)Visit(context.expression());
#pragma warning restore CS8605 // Unboxing a possibly null value.

                while (condition)
                {
                    // Visit each line in the block
                    foreach (var lineContext in context.line())
                    {
                        VisitLine(lineContext);
                    }

                    // Evaluate the expression again
#pragma warning disable CS8605 // Unboxing a possibly null value.
                    condition = (bool)Visit(context.expression());
#pragma warning restore CS8605 // Unboxing a possibly null value.
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public override object? VisitSwitchBlock([NotNull] CodeGrammarParser.SwitchBlockContext context)
        {
            // Get the expression in the switch statement
            var expression = Visit(context.expression());

            // Visit each case block
            bool isBreakExecuted = false;
            try
            {
                foreach (var caseBlockContext in context.caseBlock())
                {
                    var caseExpression = Visit(caseBlockContext.expression());

                    if (!(caseExpression is null) && !(expression is null) && caseExpression.GetType() != expression.GetType())
                    {
                        Console.WriteLine("The switch case expressions must have the same data type.");
                        return null;
                    }

                    if (caseExpression!.Equals(expression))
                    {
                        // Visit the case block
#pragma warning disable CS8605 // Unboxing a possibly null value.
                        var isBreak = (bool)VisitCaseBlock(caseBlockContext);
#pragma warning restore CS8605 // Unboxing a possibly null value.
                        // Check if the block executed a BREAK statement
                        if (isBreak)
                        {
                            isBreakExecuted = true;
                            break;
                        }
                    }
                }

                // If a BREAK statement was executed, do not execute the default block
                if (!isBreakExecuted)
                {
                    // Check if there is a default block
                    if (context.defaultBlock() is var defaultBlockContext)
                    {
                        if (defaultBlockContext is null)
                        {
                            throw new Exception("The default block must have a BREAK statement.");
                        }
                        else
                        {
                            Visit(defaultBlockContext);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public override object? VisitCaseBlock([NotNull] CodeGrammarParser.CaseBlockContext context)
        {
            try
            {
                // Process the expression in the case block
                var expression = Visit(context.expression());

                // Process each line in the case block
                foreach (var lineContext in context.line())
                {
                    Visit(lineContext);
                }

                if (context.ChildCount > 0 && context.GetChild(context.ChildCount - 2).GetText() == "BREAK")
                {
                    return true;
                }
            }catch(Exception e) { Console.WriteLine(e.Message); }

            return false;
        }
    }
}
