grammar CodeGrammar;

program
    : 'BEGIN CODE' declaration? executable_code? 'END CODE'
    ;

declaration
    : variable_declaration (DOLLAR_SIGN variable_declaration)*
    ;

executable_code
    : executable_statement (DOLLAR_SIGN executable_statement)*
    ;

variable_declaration
    : variable_name variable_type
    ;

variable_name
    : LETTER (LETTER | DIGIT | '_')*
    ;

variable_type
    : 'INT' | 'CHAR' | 'BOOL' | 'FLOAT'
    ;

executable_statement
    : variable_name ASSIGNMENT arithmetic_expression
    | variable_name ASSIGNMENT bool_expression
    | COMMENT
    ;

arithmetic_expression
    : LPAREN arithmetic_expression RPAREN
    | arithmetic_expression (MULTIPLICATION | DIVISION | MODULO) arithmetic_expression
    | arithmetic_expression (ADDITION | SUBTRACTION) arithmetic_expression
    | variable_name
    | NUMBER
    ;

bool_expression
    : bool_expression (AND | OR) bool_term
    | bool_term
    ;

bool_term
    : LPAREN bool_expression RPAREN
    | variable_name
    | TRUE
    | FALSE
    | NOT bool_term
    | bool_comparison
    ;

bool_comparison
    : arithmetic_expression (EQUAL | NOT_EQUAL | GREATER_THAN | GREATER_THAN_OR_EQUAL | LESS_THAN | LESS_THAN_OR_EQUAL) arithmetic_expression
    ;

LPAREN      : '(';
RPAREN      : ')';
MULTIPLICATION : '*';
DIVISION    : '/';
MODULO      : '%';
ADDITION    : '+';
SUBTRACTION : '-';
ASSIGNMENT  : '=';
EQUAL       : '==';
NOT_EQUAL   : '<>';
GREATER_THAN        : '>';
GREATER_THAN_OR_EQUAL : '>=';
LESS_THAN   : '<';
LESS_THAN_OR_EQUAL  : '<=';
AND         : 'AND';
OR          : 'OR';
NOT         : 'NOT';
TRUE        : 'TRUE';
FALSE       : 'FALSE';
COMMENT     : '#' ~('\r' | '\n')*;
WS          : [ \t\r\n]+ -> skip;
LETTER      : [a-zA-Z];
DIGIT       : [0-9];
NUMBER      : DIGIT+ ('.' DIGIT+)?;
DOLLAR_SIGN : '$';
