grammar MiniLang;

// ====================================================== 1. PARSER RULES
// ======================================================
program: (globalVarDecl | functionDecl)* EOF;

globalVarDecl: type ID (ASSIGN expression)? SEMI;

functionDecl: type ID LPAREN paramList? RPAREN block;

paramList: paramDecl (COMMA paramDecl)*;

paramDecl: type ID;

block: LBRACE (statement | globalVarDecl)* RBRACE;

statement:
	ifStatement
	| forStatement
	| whileStatement
	| returnStatement
	| assignment
	| block
	| SEMI;

ifStatement:
	KEYWORD_IF LPAREN expression RPAREN statement (
		KEYWORD_ELSE statement
	)?;

forStatement:
	KEYWORD_FOR LPAREN (localVarDecl | assignment)? SEMI expression? SEMI (
		assignment
		| expression
	)? RPAREN statement;

localVarDecl: type ID ASSIGN expression;

whileStatement:
	KEYWORD_WHILE LPAREN expression RPAREN statement;

returnStatement: KEYWORD_RETURN expression? SEMI;

assignment:
	ID (
		ASSIGN
		| PLUS_ASSIGN
		| MINUS_ASSIGN
		| MUL_ASSIGN
		| DIV_ASSIGN
		| MOD_ASSIGN
	) expression SEMI;

expression: logicalOrExpr;

logicalOrExpr: logicalAndExpr (OR logicalAndExpr)*;

logicalAndExpr: equalityExpr (AND equalityExpr)*;

equalityExpr:
	relationalExpr ((EQUAL | NOTEQUAL) relationalExpr)*;

relationalExpr:
	additiveExpr ((LT | GT | LE | GE) additiveExpr)*;

additiveExpr:
	multiplicativeExpr ((PLUS | MINUS) multiplicativeExpr)*;

multiplicativeExpr: unaryExpr ((STAR | DIV | MOD) unaryExpr)*;

unaryExpr: (INC | DEC)? primaryExpr // prefix
	| primaryExpr (INC | DEC)? ; // postfix

primaryExpr:
	functionCall
	| NUM
	| STRING_LITERAL
	| ID
	| LPAREN expression RPAREN;

functionCall: ID LPAREN argumentList? RPAREN;

argumentList: expression (COMMA expression)*;

type:
	KEYWORD_INT
	| KEYWORD_FLOAT
	| KEYWORD_DOUBLE
	| KEYWORD_STRING
	| KEYWORD_VOID;

// ====================================================== 2. LEXER RULES
// ======================================================
KEYWORD_INT: 'int';
KEYWORD_FLOAT: 'float';
KEYWORD_DOUBLE: 'double';
KEYWORD_STRING: 'string';
KEYWORD_VOID: 'void';
KEYWORD_IF: 'if';
KEYWORD_ELSE: 'else';
KEYWORD_FOR: 'for';
KEYWORD_WHILE: 'while';
KEYWORD_RETURN: 'return';

PLUS: '+';
MINUS: '-';
STAR: '*';
DIV: '/';
MOD: '%';

LT: '<';
GT: '>';
LE: '<=';
GE: '>=';
EQUAL: '==';
NOTEQUAL: '!=';
AND: '&&';
OR: '||';
NOT: '!';

ASSIGN: '=';
PLUS_ASSIGN: '+=';
MINUS_ASSIGN: '-=';
MUL_ASSIGN: '*=';
DIV_ASSIGN: '/=';
MOD_ASSIGN: '%=';

INC: '++';
DEC: '--';

LPAREN: '(';
RPAREN: ')';
LBRACE: '{';
RBRACE: '}';
SEMI: ';';
COMMA: ',';

// Identificatori (variabile, funcții):
ID: [a-zA-Z_][a-zA-Z0-9_]*;

NUM: [0-9]+ ('.' [0-9]+)?;

STRING_LITERAL: '"' (~["\r\n] | ('\\' .))* '"';

// Ignorăm spațiile albe
WS: [ \t\r\n]+ -> skip;

// Comentarii linie // ...
LINE_COMMENT: '//' ~[\r\n]* -> skip;

// Comentarii bloc /* ... */
BLOCK_COMMENT: '/*' .*? '*/' -> skip;