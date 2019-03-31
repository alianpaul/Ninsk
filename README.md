### 1. A Basic Expression Evaluator
The first thing that a compiler does is to break the character into words.  The parser doesn't really deal with individual characters, parser deals with words.  The first part of a compiler is a lexer which creates the words.  And it's the parser who create the sentences.

The lexer produces tokens, which you can think of as the leaves of the tree.  Ther parser produces actual sentences.  There are muliple of ways of doing parser.  80's compliers highly optimized for streaming, they try to not lex the entire input.  They will usually like consume one token at a time.  Here just run the lexer on the entire input.

Use the SyntaxNode to form the expression tree.  Once we have the tree, we can walk through it in a genetic way.

Needs a more pretty printer.