grammar CodeGrammar;

program: block EOF;

block: BEGIN_CODE declare* line* END_CODE;

declare: type ID ('=' exp)? (',' ID ('=' exp)?)* NEW_LINE;

line: (statement | if_block | while_block) NEW_LINE;

statement: assignment | call;

assignment: ID ('=' ID)* '=' exp;

if_block: 'IF' '(' exp ')' BEGIN_IF line* END_IF else_if_block?;

else_if_block: 'ELSE' (BEGIN_IF line* END_IF) | if_block;

while_block: WHILE '(' exp ')' NEW_LINE BEGIN_WHILE NEW_LINE line* END_WHILE NEW_LINE;

call: DISPLAY (exp (',' exp)*)? | SCAN ID (',' ID)*;

exp: const | ID | call | '(' exp ')' | 'NOT' exp | unary exp | exp mult_op exp | exp add_op exp | exp compare_op exp | exp logic_op exp | exp concat exp | NEXTLINE;

const: INTEGER_VALUE | FLOAT_VALUE | CHAR_VALUE | BOOL_VALUE | STRING_VALUE;

type: INT | FLOAT | BOOL | CHAR | STRING;

mult_op: '*' | '/' | '%';

add_op: '+' | '-';

compare_op: '==' | '<>' | '>' | '<' | '>=' | '<=';

unary: '+' | '-';

concat: '&';

logic_op: LOGICAL_OPERATOR;

LOGICAL_OPERATOR: 'AND' | 'OR' ;

BEGIN_CODE: NEW_LINE? 'BEGIN CODE' NEW_LINE;

END_CODE: 'END CODE' NEW_LINE?;

BEGIN_IF: 'BEGIN IF';

END_IF: 'END IF';

WHILE: 'WHILE';

BEGIN_WHILE: 'BEGIN WHILE';

END_WHILE: 'END WHILE';

INT: 'INT';

FLOAT: 'FLOAT';

CHAR: 'CHAR';

BOOL: 'BOOL';

STRING: 'STRING';

ID: [a-zA-Z_][a-zA-Z0-9_]*;

INTEGER_VALUE: [0-9]+;

FLOAT_VALUE: [0-9]+ '.' [0-9]+;

STRING_VALUE: '"' ( ~('"' | '\\') | '\\' . )* '"';

BOOL_VALUE: '"TRUE"' | '"FALSE"';

CHAR_VALUE: ('\'' ~[\r\n\'] '\'') | '[' .? ']';

DISPLAY: 'DISPLAY:';

SCAN: 'SCAN:';

NEXTLINE: '$';

COMMENT: '#' ~[\r\n]* NEW_LINE? -> skip;

NEW_LINE: [\r?\n]+;

WS: [ \t\r]+ -> skip;
