﻿grammar CodeGrammar;

program: NEWLINE? BEGIN_CODE NEWLINE? initialization* variable* line* NEWLINE? END_CODE NEWLINE? EOF;

BEGIN_CODE: 'BEGIN CODE' ;
END_CODE: 'END CODE' ;

line
    : singleAssignment
    | multipleAssignments
    | display
    | scan
    | ifBlock
    | elseIfBlock
    | elseBlock
    | whileBlock
    | switchBlock
    | caseBlock
    | defaultBlock
    ;

initialization: type IDENTIFIER (',' IDENTIFIER)* ('=' expression)? NEWLINE?;
variable: type IDENTIFIER (EQUALS expression)? NEWLINE?
        | type IDENTIFIER (EQUALS expression)? (',' IDENTIFIER (EQUALS expression)?)* NEWLINE?
        ;
singleAssignment: IDENTIFIER '=' expression NEWLINE?;
multipleAssignments: IDENTIFIER ('=' IDENTIFIER)* '=' expression NEWLINE? 
                   | IDENTIFIER ('=' IDENTIFIER)* '=' expression (',' singleAssignment)* NEWLINE?
                   ;

BEGIN_IF: 'BEGIN IF' ;
END_IF: 'END IF' ;
ifBlock: 'IF' '('expression')' NEWLINE BEGIN_IF NEWLINE line* NEWLINE END_IF NEWLINE? (elseBlock | elseIfBlock)? ;
elseIfBlock: 'ELSE IF' '('expression')' NEWLINE BEGIN_IF NEWLINE line* NEWLINE END_IF NEWLINE? (elseBlock | elseIfBlock)? ;
elseBlock: 'ELSE' NEWLINE BEGIN_IF NEWLINE line* NEWLINE END_IF NEWLINE?;

WHILE: 'WHILE' ;
BEGIN_WHILE: 'BEGIN WHILE' ;
END_WHILE: 'END WHILE' ;
whileBlock: WHILE '(' expression ')' NEWLINE BEGIN_WHILE NEWLINE line* NEWLINE END_WHILE NEWLINE? ;

switchBlock : 'SWITCH' '(' expression ')' NEWLINE 'BEGIN SWITCH' NEWLINE caseBlock+ defaultBlock? 'END SWITCH' NEWLINE?;
caseBlock : 'CASE' expression NEWLINE line* NEWLINE 'BREAK' NEWLINE?;
defaultBlock : 'DEFAULT' NEWLINE line* NEWLINE?;

DISPLAY: 'DISPLAY:';
display: DISPLAY ((expression (concat | NEXTLINE)* expression?)*)? NEWLINE?;
SCAN: 'SCAN:';
scan: SCAN IDENTIFIER (',' IDENTIFIER)* NEWLINE?;

type: INT | FLOAT | BOOL | CHAR ;
INT: 'INT' ;
FLOAT: 'FLOAT';
CHAR: 'CHAR';
BOOL: 'BOOL';

constant: INTEGER_VALUES | FLOAT_VALUES | CHARACTER_VALUES | BOOLEAN_VALUES | STRING_VALUES ;
INTEGER_VALUES: ('-')? [0-9]+ ;
FLOAT_VALUES: ('-')? [0-9]+ '.' [0-9]+ ;
CHARACTER_VALUES: ('\'' ~[\r\n\'] '\'') | '[' .? ']' ;
BOOLEAN_VALUES:  '\"TRUE\"' | '\"FALSE\"' ;
STRING_VALUES: '"' ( ~('"' | '\\') | '\\' . )* '"';

expression
    : constant                                                  #constantValueExpression
    | IDENTIFIER                                                #identifierExpression
    | COMMENT                                                  #commentExpression
    | display                                                   #displayExpression
    | scan                                                      #scanExpression
    | '-' expression                                            #negativeExpression
    | '(' expression ')'                                        #parenthesisExpression
    | 'NOT' expression                                          #notExpression
    | expression highPrecedenceOperator expression              #multDivModExpression
    | expression lowPrecedenceOperator expression               #addSubConcatenatorExpression
    | expression comparisonOperator expression                  #comparisonExpression
    | expression logicalOperator expression                     #logicalExpression
    | expression concat expression                              #concatExpression
    | NEXTLINE                                                  #nextLineExpression
    ; 

highPrecedenceOperator: '*' | '/' | '%' ;
lowPrecedenceOperator: '+' | '-' | '&' | NEXTLINE ;
comparisonOperator: '==' | '<>' | '>' | '<' | '>=' | '<='  ;
logicalOperator: 'AND' | 'OR' | 'NOT' ;
concat: '&' ;

EQUALS: '=';
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]* ;
COMMENT: '#' ~[\r\n]* NEWLINE -> channel(HIDDEN) ;
NEXTLINE: '$' ;
WHITESPACES: [ \t\r]+ -> skip ;
NEWLINE: [\r?\n]+ ;