grammar ScratchScript;
/*
Parser
*/

program: line* EOF;
line: (statement | ifStatement | whileStatement | attributeStatement | functionDeclarationStatement | comment);
statement: (assignmentStatement | functionCallStatement | variableDeclarationStatement | importStatement | returnStatement) Semicolon;

assignmentStatement: Identifier assignmentOperators expression;
variableDeclarationStatement: VariableDeclare Identifier Assignment expression;
functionCallStatement: Identifier LeftParen (expression (Comma expression)*?) RightParen;
functionDeclarationStatement: FunctionDeclare Identifier LeftParen (functionArgumentDeclaration (Comma functionArgumentDeclaration)*?)? RightParen functionReturnType? block; 
ifStatement: If expression block (Else elseIfStatement)?;
whileStatement: While expression block;
elseIfStatement: block | ifStatement;
importStatement: Import String;
attributeStatement: At Identifier;
returnStatement: Return expression;
functionArgumentDeclaration: Identifier Colon type;
functionReturnType: Arrow type;

expression
    : constant #constantExpression
    | Identifier #identifierExpression
    | functionCallStatement #functionCallExpression
    | LeftParen expression RightParen #parenthesizedExpression
    | Not expression #notExpression
    | addOperators expression #unaryAddExpression
    | expression multiplyOperators expression #binaryMultiplyExpression
    | expression addOperators expression #binaryAddExpression
    | expression compareOperators expression #binaryCompareExpression
    | expression booleanOperators expression #binaryBooleanExpression
    ;

multiplyOperators: Multiply | Divide | Power | Modulus;
addOperators: Plus | Minus;
compareOperators: Equal | NotEqual | Greater | GreaterOrEqual | Lesser | LesserOrEqual;
booleanOperators: And | Or | Xor;
assignmentOperators: Assignment | AdditionAsignment | SubtractionAssignment | MultiplicationAssignment | DivisionAssignment | ModulusAssignment;
type: ('int' | 'float' | 'string' | 'color' | 'bool' | 'void');

block: LeftBrace line* RightBrace;

constant: Integer | Float | String | Boolean | Color | Angle;
comment: Comment;

/*
    Lexer fragments
*/

fragment Digit: [0-9];
fragment HexDigit: [0-9A-F];
Whitespace: (' ' | '\t') -> channel(HIDDEN);
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
Arrow: '->';
Colon: ':';

SingleLineCommentStart: '//';
MultiLineCommentStart: '/*';
MultiLineCommentEnd: '*/';

Comment
    :   ( SingleLineCommentStart (~[\r\n]|Whitespace)* 
        | MultiLineCommentStart .*? MultiLineCommentEnd
        )
    ;

At: '@';
Hashtag: '#';

Multiply: '*';
Plus: '+';
Minus: '-';
Divide: '/';
Power: '**';
Modulus: '%';

And: '&&';
Or: '||';
Xor: '^';

//<assoc=right>
Greater: '>';
Lesser: '<';
GreaterOrEqual: '>=';
LesserOrEqual: '<=';
Equal: '==';
NotEqual: '!=';

AdditionAsignment: '+=';
SubtractionAssignment: '-=';
MultiplicationAssignment: '*=';
DivisionAssignment: '/=';
ModulusAssignment: '%=';

/*
    Keywords
*/
If: 'if' Whitespace+;

/*Very important for newlines:

else
{
}
and
else if ...
{
}
and
else {
}

*/
Else: 'else';

While: 'while' Whitespace+;
True: 'true' Whitespace+;
False: 'false' Whitespace+;
VariableDeclare: 'var' Whitespace+;
Import: 'import' Whitespace+;
FunctionDeclare: 'function' Whitespace+;
Return: 'return' Whitespace+;

/*
    Lexer rules
*/
Boolean: True | False;
Integer: Minus? Digit+;
Float: Digit+ ([.] Digit+)? 'f'; 
Identifier: [a-zA-Z_][a-zA-Z0-9_]*;
String: ('"' ~'"'* '"') | ('\'' ~'\''* '\'');
Color: Hashtag HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit;