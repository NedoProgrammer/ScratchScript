grammar ScratchScript;
/*
Parser
*/

program: line* EOF;
line: statement | ifStatement | whileStatement | singleLineComment | multiLineComment;
statement: (assignmentStatement | functionCallStatement | variableDeclarationStatement | importStatement | attributeStatement) Semicolon;

assignmentStatement: Identifier Assignment expression;
variableDeclarationStatement: VariableDeclare Identifier Assignment expression;
functionCallStatement: Identifier LeftParen (expression (Comma expression)*?) RightParen;
ifStatement: If expression block (Else elseIfStatement)?;
whileStatement: While expression block;
elseIfStatement: block | ifStatement;
importStatement: Import String;
attributeStatement: At Identifier;

expression
    : constant #constantExpression
    | Identifier #identifierExpression
    | functionCallStatement #functionCallExpression
    | LeftParen expression RightParen #parenthesizedExpression
    | Not expression #notExpression
    | expression multiplyOperators expression #binaryMultiplyExpression
    | expression addOperators expression #binaryAddExpression
    | expression compareOperators expression #binaryCompareExpression
    | expression booleanOperators expression #binaryBooleanExpression
    ;

multiplyOperators: Multiply | Divide | Power | Modulo;
addOperators: Plus | Minus;
compareOperators: Equal | NotEqual | Greater | GreaterOrEqual | Lesser | LesserOrEqual;
booleanOperators: And | Or | Xor;

block: LeftBrace line* RightBrace;

constant: Integer | Float | String | Boolean | Color | Angle;

singleLineComment: SingleLineComment .*?;
multiLineComment: MultiLineCommentStart (.*?) MultiLineCommentEnd;

/*
    Lexer fragments
*/

fragment Digit: [0-9];
fragment HexDigit: [0-9A-F];
Whitespace: (' ' | '\t') -> skip;
NewLine: ('\r'? '\n' | '\r' | '\n') -> skip;
Semicolon: ';';
LeftParen: '(';
RightParen: ')';
LeftBracket: '[';
RightBracket: ']';
LeftBrace: '{';
RightBrace: '}';
Assignment: '=';
Comma: ',';
Not: '!';
SingleLineComment: '//';
MultiLineCommentStart: '/*';
MultiLineCommentEnd: '*/';
At: '@';
Hashtag: '#';

Multiply: '*';
Plus: '+';
Minus: '-';
Divide: '/';
Power: '**';
Modulo: '%';

And: '&&';
Or: '||';
Xor: '^';

Greater: '>';
Lesser: '<';
GreaterOrEqual: '>=';
LesserOrEqual: '<=';
Equal: '==';
NotEqual: '!=';

/*
    Keywords
*/
If: 'if' Whitespace+;
Else: 'else' Whitespace+;
While: 'while' Whitespace+;
True: 'true' Whitespace+;
False: 'false' Whitespace+;
VariableDeclare: 'var' Whitespace+;
Import: 'import' Whitespace+;
FunctionDeclare: 'function' Whitespace+;
Degrees: 'deg';
Radians: 'rad';

/*
    Lexer rules
*/
Boolean: True | False;
Integer: Digit+;
Float: Digit ([.] Digit+)? 'f'; 
Identifier: [a-zA-Z_][a-zA-Z0-9_]*;
String: ('"' ~'"'* '"') | ('\'' ~'\''* '\'');
Color: Hashtag HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit;
Angle: Integer (Degrees | Radians);