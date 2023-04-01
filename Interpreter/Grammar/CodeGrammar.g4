grammar CodeGrammar;

program: NEWLINE? BEGIN_CODE NEWLINE line* NEWLINE END_CODE NEWLINE?;

BEGIN_CODE: 'BEGIN CODE' ;
END_CODE: 'END CODE' ;

line
    : initialization
	| variable
    | assignment
    | ifBlock 
    | whileBlock
	| call
    | display
    | scan
	| COMMENTS
    ;

initialization: type IDENTIFIER (',' IDENTIFIER)* ('=' expression)? NEWLINE?;
variable: type IDENTIFIER ('=' expression)? NEWLINE?;
assignment: IDENTIFIER '=' expression NEWLINE?;

BEGIN_IF: 'BEGIN IF' ;
END_IF: 'END IF' ;
ifBlock: 'IF' '('expression')' NEWLINE BEGIN_IF NEWLINE line* NEWLINE END_IF NEWLINE? elseIfBlock? ;
elseIfBlock: 'ELSE' (NEWLINE BEGIN_IF NEWLINE line* NEWLINE END_IF) | ifBlock ;

WHILE: 'WHILE' ;
BEGIN_WHILE: 'BEGIN WHILE' ;
END_WHILE: 'END WHILE' ;
whileBlock: WHILE '(' expression ')' NEWLINE BEGIN_WHILE NEWLINE line* NEWLINE END_WHILE ;

call: IDENTIFIER ':' (expression (',' expression)*)? ;
DISPLAY: 'DISPLAY:';
display: NEWLINE? DISPLAY IDENTIFIER ('&' IDENTIFIER)* ('$' NEWLINE)? ;
SCAN: 'SCAN:';
scan: SCAN IDENTIFIER (',' IDENTIFIER)* ;

type: INT | FLOAT | BOOL | CHAR ;
INT: 'INT' ;
FLOAT: 'FLOAT';
CHAR: 'CHAR';
BOOL: 'BOOL';

constant: INTEGER_VALUES | FLOAT_VALUES | CHARACTER_VALUES | BOOLEAN_VALUES | STRING_VALUES ;
INTEGER_VALUES: [0-9]+ ;
FLOAT_VALUES: [0-9]+ '.' [0-9]+ ;
CHARACTER_VALUES: '\'' ~[\r\n\'] '\'' ;
BOOLEAN_VALUES:  '\"TRUE\"' | '\"FALSE\"' ;
STRING_VALUES: ('"' ~'"'* '"') | ('\'' ~'\''* '\'') ;

expression
    : constant                                            #constantValueExpression
    | IDENTIFIER                                               #identifierExpression
    | COMMENTS                                                  #commentExpression
    | call                                                #methodCallExpression
    | '(' expression ')'                                        #parenthesisExpression
    | 'NOT' expression                                          #notExpression
    | expression highPrecedenceOperator expression                 #multDivModExpression
    | expression lowPrecedenceOperator expression         #addSubConcatenatorExpression
    | expression comparisonOperator expression                 #comparisonExpression
    | expression logicalOperator expression                    #logicalExpression
    | escapeCodeOpen expression escapeCodeClose                 #escapeCodeExpression
    ; 

highPrecedenceOperator: '*' | '/' | '%' ;
lowPrecedenceOperator: '+' | '-' | '&' ;
comparisonOperator: '==' | '<>' | '>' | '<' | '>=' | '<='  ;
logicalOperator: 'AND' | 'OR' | 'NOT' ;
escapeCodeOpen: '[' ;
escapeCodeClose: ']' ;

IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]* ;
COMMENTS: '#' ~[\r\n]* -> skip ;
NEXTLINE: '$' ;
WHITESPACES: [ \t\r]+ -> skip ;
NEWLINE: '\r'? '\n'| '\r';