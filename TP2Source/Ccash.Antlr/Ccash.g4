grammar Ccash;

/***************************************************
 *                                                 *
 *                      Parser                     *
 *                                                 *
 **************************************************/
 
compilationUnit : (functionDeclaration | structDeclaration)* EOF;

functionDeclaration : functionHeader block;

functionHeader : 'func' Identifier '(' functionParameters? ')' type?;

functionParameters : variable (',' variable)*;
    
functionCall : functionName '(' functionArgs? ')';

functionArgs : expression (',' expression)*;

structDeclaration : ('struct' | 'class') Identifier '{' (field | method)* '}';

field : type Identifier ';';

method : methodHeader block;

methodHeader : 'func' ('const' | 'mut' | 'static') Identifier '(' functionParameters? ')' type?;

variableDeclaration : variable ':=' expression;

variable : variableType Identifier;

functionName
    : Identifier
    | Bool
    | Int8
    | Uint8
    | Int16
    | Uint16
    | Int32
    | Uint32
    | Int64
    | Uint64
    | Float32
    | Float64;

/***************************************************
 *                                                 *
 *                      Types                      *
 *                                                 *
 **************************************************/

valueType 
    : 'bool'                            # BoolType
    | 'int8'                            # IntegralType
    | 'uint8'                           # IntegralType
    | 'int16'                           # IntegralType
    | 'uint16'                          # IntegralType
    | 'int32'                           # IntegralType
    | 'uint32'                          # IntegralType
    | 'int64'                           # IntegralType
    | 'uint64'                          # IntegralType
    | 'float32'                         # FloatType
    | 'float64'                         # FloatType
    | Identifier                        # StructType
    ;
    
referenceType : 'ref' valueType;

arrayType : '[]' (valueType | referenceType | arrayType);

variableType : ('const' | 'mut') (valueType | referenceType | arrayType);

type : valueType | (('const' | 'mut') (referenceType | arrayType));

/***************************************************
 *                                                 *
 *                   Statements                    *
 *                                                 *
 **************************************************/

statement 
    : block                                                 # BlockStatement
    | ifStatement                                           # ConditionalStatement
    | expression '.' Identifier '(' functionArgs? ')' ';'   # MethodCallStatement
    | functionCall ';'                                      # FunctionCallStatement
    | variableDeclaration ';'                               # VariableDeclarationStatement
    | reassignment ';'                                      # ReassignmentStatement
    | whileStatement                                        # LoopStatement
    | doStatement                                           # LoopStatement
    | forStatement                                          # LoopStatement
    | repeatStatement                                       # LoopStatement
    | returnStatement                                       # ControlFlowStatement
    | breakStatement                                        # ControlFlowStatement;

block : '{' statement* '}';

ifStatement : 'if' expression block elseIfStatement* elseStatement?;

elseIfStatement : 'else' 'if' expression block;

elseStatement : 'else' block;

returnStatement : 'return' expression? ';';

reassignment 
    : expression ':=:' expression
    | expression ':=' expression
    | expression ('+='|'-='|'/='|'*=') expression
    | expression ('%='|'&='|'^='|'|=') expression
    | expression ('--' | '++');

breakStatement : 'break;';

loopBlock : '{' statement* '}';
    
whileHeader : 'while' expression;

whileStatement : whileHeader loopBlock;

doStatement : 'do' loopBlock whileHeader ';';

forHeader : 'for' forInitialization? ';' expression? ';' forUpdate?;

forStatement : forHeader loopBlock; 

repeatHeader : 'repeat' expression;

repeatStatement : repeatHeader loopBlock;

forInitialization
    : functionCall
    | variableDeclaration
    | reassignment;

forUpdate
    : functionCall
    | reassignment;

/***************************************************
 *                                                 *
 *                   Expressions                   *
 *                                                 *
 **************************************************/

expression
    : '(' expression ')'                                                # ParenthesisExpression
    | IntegerLiteral                                                    # IntegerLiteralExpression
    | UintegerLiteral                                                   # IntegerLiteralExpression
    | FloatLiteral                                                      # FloatLiteralExpression
    | Float32Literal                                                    # Float32LiteralExpression
    | StringLiteral                                                     # StringLiteralExpression
    | Identifier '{' (fieldInitializer (',' fieldInitializer)*)? '}'    # StructLiteralExpression
    | Identifier                                                        # IdentifierExpression
    | ('true' | 'false')                                                # BooleanLiteralExpression
    | expression '.' Identifier '(' functionArgs? ')'                   # MethodCallExpression
    | functionCall                                                      # FunctionCallExpression
    | expression '.' Identifier                                         # MemberAccessExpression
    | '[' expression (',' expression)* ']'                              # ArrayLiteralExpression
    | expression '[' expression ']'                                     # IndexOperatorExpression
    | 'ref' Identifier                                                  # RefExpression
    | ('-' | '!') expression                                            # UnaryOperatorExpression
    | expression ('*' | '/' | '%') expression                           # BinaryOperatorExpression
    | expression ('+' | '-') expression                                 # BinaryOperatorExpression
    | expression ('<<' | '>>') expression                               # BinaryOperatorExpression
    | expression ('<' | '<=' | '>' | '>=') expression                   # BinaryOperatorExpression
    | expression ('==' | '!=') expression                               # BinaryOperatorExpression
    | expression '&' expression                                         # BinaryOperatorExpression
    | expression '^' expression                                         # BinaryOperatorExpression
    | expression '|' expression                                         # BinaryOperatorExpression
    | expression '&&' expression                                        # BinaryOperatorExpression
    | expression '||' expression                                        # BinaryOperatorExpression
    | expression '?' expression ':' expression                          # TernaryExpression;
    
fieldInitializer : Identifier ':' expression;

/***************************************************
 *                                                 *
 *                      Lexer                      *
 *                                                 *
 **************************************************/

Int8    : 'int8';
Uint8   : 'uint8';
Int16   : 'int16';
Uint16  : 'uint16';
Int32   : 'int32';
Uint32  : 'uint32';
Int64   : 'int64';
Uint64  : 'uint64';

Float32 : 'float32';
Float64 : 'float64';

Bool    : 'bool';
True    : 'true';
False   : 'false';

LeftParen       : '(';
RightParen      : ')';
LeftBracket     : '[';
RightBracket    : ']';
LeftBrace       : '{';
RightBrace      : '}';
DoubleQuote     : '"';
SingleQuote     : '\'';

Return  : 'return';
Func    : 'func';
Const   : 'const';
Mut     : 'mut';
Static  : 'static';
If      : 'if';
Else    : 'else';
While   : 'while';
Do      : 'do';
For     : 'for';
As      : 'as';
Ref     : 'ref';
Class   : 'class';
Struct  : 'struct';
Repeat  : 'repeat';

Plus        : '+';
Minus       : '-';
Mul         : '*';
Div         : '/';
Mod         : '%';
BitAnd      : '&';
BitOr       : '|';
BitXor      : '^';
Not         : '!';
LogicalAnd  : '&&';
LogicalOr   : '||';
Lesser      : '<';
LesserEq    : '<=';
Greater     : '>';
GreaterEq   : '>=';
Equal       : '==';
NotEqual    : '!=';
Ternary     : '?';
Swap        : ':=:';
Assign      : ':=';
AddAssign   : '+=';
SubAssign   : '-=';
DivAssign   : '/=';
MulAssign   : '*=';
ModAssign   : '%=';
AndAssign   : '&=';
OrAssign    : '|=';
XorAssign   : '^=';

Semicolon : ';';

Identifier : Letter (Letter | Digit | ModuleSeparator)*;

fragment Letter : [a-zA-Z_];

fragment Digit : [0-9];

ModuleSeparator : '::';

IntegerLiteral : '-'?Digit+;

UintegerLiteral : Digit+;   ////

FloatLiteral : '-'?Digit+'.'Digit+;

Float32Literal : FloatLiteral'f';

StringLiteral : '"' .*? '"';

Whitespace : [ \t]+ -> skip;

Newline : ('\r' '\n'? | '\n') -> skip;

BlockComment : '/*' .*? '*/' -> skip;

LineComment : '//' ~[\r\n]* -> skip;
