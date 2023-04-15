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
                    throw new ArgumentException($"Variable '{varName}' is already defined!");
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
            var type = VisitType(context.type());

            // Loop over all the identifiers and add them to the dictionary
            for (int i = 0; i < context.IDENTIFIER().Length; i++)
            {
                var identifier = context.IDENTIFIER(i).GetText();
                var expression = context.expression(i) != null ? Visit(context.expression(i)) : null;
                var variable = expression != null ? expression : null;

                // Throw an exception if variable is declared without initialization value
                if (variable == null)
                {
                    throw new Exception($"Variable {identifier} is declared without an initialization value.");
                }

                _variables[identifier] = variable;
            }

            return null;
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

        public override object? VisitNextLineExpression([NotNull] CodeGrammarParser.NextLineExpressionContext context)
        {
            return "\n";
        }

        public override object? VisitConcatExpression([NotNull] CodeGrammarParser.ConcatExpressionContext context)
        {
            var left = Visit(context.expression(0));
            var right = Visit(context.expression(1));

            return $"{left}{right}";
        }
        public override object? VisitParenthesisExpression([NotNull] CodeGrammarParser.ParenthesisExpressionContext context)
        {
            // Visit the inner expression and return its result
            return Visit(context.expression());
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

        public override object? VisitAddSubConcatenatorExpression([NotNull] CodeGrammarParser.AddSubConcatenatorExpressionContext context)
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
                else if(op == "$")
                {
                    return leftInt.ToString() + "\n" + rightInt.ToString();
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
                    return leftFloat.ToString("0.0###############") + rightFloat.ToString("0.0###############");
                }
                else if (op == "$")
                {
                    return leftFloat.ToString("0.0###############") + "\n" + rightFloat.ToString("0.0###############");
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
                    return leftInt2.ToString() + rightFloat2.ToString("0.0###############");
                }
                else if (op == "$")
                {
                    return leftInt2.ToString() + "\n" + rightFloat2.ToString("0.0###############");
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
                    return leftFloat2.ToString("0.0###############") + rightInt2.ToString();
                }
                else
                {
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
                    throw new ArgumentException($"Unknown operator: {op}");
                }
            }
            else if(left is char leftChar1 && right is char rightChar1)
            {
                if (op == "&")
                {
                    return leftChar1.ToString() + rightChar1;
                }
                else if(op == "$")
                {
                    return leftChar1.ToString() + "\n" + rightChar1;
                }
                else
                {
                    throw new ArgumentException($"Unknown operator: {op}");
                }
            }
            else if(left is char leftChar2 && right is int rightInt4)
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
                else
                {
                    throw new ArgumentException($"Unknown operator: {op}");
                }
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
                else
                {
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
                    throw new ArgumentException($"Unknown operator: {op}");
                }
            }
            else if(right == null && op == "$")
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
        }

        public override object? VisitComparisonExpression(CodeGrammarParser.ComparisonExpressionContext context)
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

        public override object? VisitLogicalExpression([NotNull] CodeGrammarParser.LogicalExpressionContext context)
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
        }

        public override object? VisitIfBlock([NotNull] CodeGrammarParser.IfBlockContext context)
        {
            // Evaluate the expression inside the if statement
            bool condition = (bool)Visit(context.expression());

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

            return null;
        }
    }
}
