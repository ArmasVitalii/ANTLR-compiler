grammar MiniLang;

// ====================================================== PARSER RULES
// ======================================================

program: (structDecl | globalVarDecl | functionDecl)* EOF;

/* 1) DEFINIRE STRUCT */
structDecl: KEYWORD_STRUCT ID LBRACE structBody RBRACE;

structBody: (structFieldDecl | functionDecl)*; // câmpuri și funcții

structFieldDecl: type ID SEMI; // ex: int x;

/* 2) VARIABILE GLOBALE */
globalVarDecl: type ID (ASSIGN expression)? SEMI;

/* 3) DECLARAȚII DE FUNCȚII */
functionDecl: type ID LPAREN paramList? RPAREN block;

paramList: paramDecl (COMMA paramDecl)*;

paramDecl: type ID;

block: LBRACE (statement | globalVarDecl)* RBRACE;

/* 4) INSTRUCȚIUNI */
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

forStatement: // Permitem fie assignment/localVarDecl înainte de primul ';', fie nimic
	KEYWORD_FOR LPAREN (assignment | localVarDecl)? SEMI expression? SEMI (
		assignment
		| expression
	)? RPAREN statement;

// localVarDecl folosit doar în for: (ex: int i = 0)
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

/* 5) EXPR */
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

// Permitem inc/dec prefix/postfix
unaryExpr: (INC | DEC)? primaryExpr | primaryExpr (INC | DEC)?;

/* Apel de funcție cu argumente multiple */
primaryExpr:
	functionCall
	| NUM
	| STRING_LITERAL
	| ID
	| LPAREN expression RPAREN;

functionCall: ID LPAREN argumentList? RPAREN;

argumentList: expression (COMMA expression)*;

/* 6) TIPURI */
type:
	KEYWORD_INT
	| KEYWORD_FLOAT
	| KEYWORD_DOUBLE
	| KEYWORD_STRING
	| KEYWORD_VOID;

// ====================================================== LEXER RULES
// ======================================================
KEYWORD_STRUCT: 'struct';
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

ID: [a-zA-Z_][a-zA-Z0-9_]*;

/* suport minimal float/double */
NUM: [0-9]+ ('.' [0-9]+)?;

STRING_LITERAL: '"' (~["\r\n] | ('\\' .))* '"';

WS: [ \t\r\n]+ -> skip;

LINE_COMMENT: '//' ~[\r\n]* -> skip;

BLOCK_COMMENT: '/*' .*? '*/' -> skip;