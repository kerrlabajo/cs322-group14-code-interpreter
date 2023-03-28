using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;

namespace Interpreter.Grammar
{
public class CodeVisitor : CodeGrammarBaseVisitor<object>
{
    public override object VisitProgram(CodeGrammarParser.ProgramContext context)
    {
        // Handle program node
        // Visit declaration and executable code sub-nodes
        return base.VisitProgram(context);
    }

    public override object VisitDeclaration(CodeGrammarParser.DeclarationContext context)
    {
        // Handle variable declaration node
        // Visit variable name and type sub-nodes
        return base.VisitDeclaration(context);
    }

    public override object VisitExecutable_code(CodeGrammarParser.Executable_codeContext context)
    {
        // Handle executable code node
        // Visit executable statement sub-nodes
        return base.VisitExecutable_code(context);
    }

    public override object VisitVariable_declaration(CodeGrammarParser.Variable_declarationContext context)
    {
        // Handle variable declaration node
        // Access the variable name and type by calling the corresponding
        // child nodes using context.variable_name() and context.variable_type()
        return base.VisitVariable_declaration(context);
    }

    public override object VisitExecutable_statement(CodeGrammarParser.Executable_statementContext context)
    {
        // Handle executable statement node
        // Access the variable name, arithmetic expression and bool expression
        // by calling the corresponding child nodes using context.variable_name(),
        // context.arithmetic_expression() and context.bool_expression()
        return base.VisitExecutable_statement(context);
    }

    public override object VisitArithmetic_expression(CodeGrammarParser.Arithmetic_expressionContext context)
    {
        // Handle arithmetic expression node
        // Access the variable name and number by calling the corresponding
        // child nodes using context.variable_name() and context.NUMBER()
        // Recursively visit child nodes for nested expressions
        return base.VisitArithmetic_expression(context);
    }

    public override object VisitBool_expression(CodeGrammarParser.Bool_expressionContext context)
    {
        // Handle bool expression node
        // Recursively visit child nodes for nested expressions
        return base.VisitBool_expression(context);
    }

    public override object VisitBool_term(CodeGrammarParser.Bool_termContext context)
    {
        // Handle bool term node
        // Access the variable name and boolean values by calling the corresponding
        // child nodes using context.variable_name(), context.TRUE() and context.FALSE()
        // Recursively visit child nodes for nested expressions
        return base.VisitBool_term(context);
    }

    public override object VisitBool_comparison(CodeGrammarParser.Bool_comparisonContext context)
    {
        // Handle bool comparison node
        // Access the arithmetic expressions by calling the corresponding
        // child nodes using context.arithmetic_expression()
        return base.VisitBool_comparison(context);
    }
}
}
