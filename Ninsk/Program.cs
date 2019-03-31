using System;
using System.Collections.Generic;
using System.Linq;

namespace Ninsk
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write(">");
                var line  = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    return;

                //var lexer = new Lexer(line);
                var parser = new Parser(line);
                var expression = parser.Parse();

                PrettyPrint(expression);
                
                /*
                while (true)
                {
                    var token = lexer.NextToken();
                    if (token.Kind == SyntaxKind.EndOfFileToken)
                        break;

                    Console.Write($"{token.Kind}: {token.Text} {token.Value}\n");
                }
                */

            }
        }

        static void PrettyPrint(SyntaxNode node, string indent = "")
        {

            Console.Write(indent);

            Console.Write(node.Kind);
            if(node is SyntaxToken t && t.Value != null)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }

            Console.WriteLine();
            indent += "    ";
            foreach(var child in node.GetChildren())
            {
                PrettyPrint(child, indent);
            }
        }
    }

    enum SyntaxKind
    {
        PlusToken,
        NumberToken,
        WhiteSpaceToken,
        MinusToken,
        MultiplyToken,
        DivideToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        BadToken,
        EndOfFileToken,
        NumberExpression,
        BinaryExpression
    }

    class SyntaxToken : SyntaxNode //Tokens are actually leaves in our tree.
    {
        public SyntaxToken(SyntaxKind kind, int position, string text, object value)
        {
            Kind     = kind;
            Position = position;
            Text     = text;
            Value    = value; 
        }

        public override SyntaxKind Kind { get; }
        public int        Position { get; }
        public string     Text { get; }
        public object     Value { get;  }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();
        }
    }

    class Lexer
    {
        private readonly string _text;
        private int _position;

        public Lexer(string text)
        {
            _text = text;
        }

        private char Current
        {
            get
            {
                if (_position >= _text.Length)
                    return '\0';

                return _text[_position];
            }
        }

        private void Next()
        {
            _position++;
        }

        public SyntaxToken NextToken()
        {

            if (_position >= _text.Length)
                return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0", null);

            // Number
            if(char.IsDigit(Current))
            {
                var start = _position;

                while (char.IsDigit(Current))
                    Next();

                var text = _text.Substring(start, _position - start);
                int.TryParse(text, out var value);
                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
            }

            // WhiteSpace
            if (char.IsWhiteSpace(Current))
            {
                var start = _position;

                while (char.IsWhiteSpace(Current))
                    Next();

                var text = _text.Substring(start, _position - start);
                return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, text, null);
            }

            if (Current == '+')
                return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null);
            else if (Current == '-')
                return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", null);
            else if (Current == '*')
                return new SyntaxToken(SyntaxKind.MultiplyToken, _position++, "*", null);
            else if (Current == '/')
                return new SyntaxToken(SyntaxKind.DivideToken, _position++, "/", null);
            else if (Current == '(')
                return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(", null);
            else if (Current == ')')
                return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, ")", null);

            return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);
        }
    }

    abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }
        public abstract IEnumerable<SyntaxNode> GetChildren();
    }

    abstract class ExpressionSyntax : SyntaxNode
    {
    }

    sealed class NumberExperssionSyntax : ExpressionSyntax
    {
        public NumberExperssionSyntax(SyntaxToken numberToken)
        {
            NumberToken = numberToken;
        }
        public override SyntaxKind Kind => SyntaxKind.NumberExpression;
        
        public SyntaxToken NumberToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NumberToken; //return number token
        }
    }

    sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }
        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

        public ExpressionSyntax Left { get; private set; }
        public SyntaxToken OperatorToken { get; private set; }
        public ExpressionSyntax Right { get; private set; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }
    }

    class Parser
    {
        private SyntaxToken[] _tokens;
        private int           _position;

        public Parser(string text)
        {
            var lexer = new Lexer(text);
            var tokens = new List<SyntaxToken>();

            do
            {
                var token = lexer.NextToken();

                if (token.Kind != SyntaxKind.WhiteSpaceToken && token.Kind != SyntaxKind.BadToken)
                    tokens.Add(token);

            } while (tokens[tokens.Count - 1].Kind != SyntaxKind.EndOfFileToken);

            _tokens = tokens.ToArray();
        }

        private SyntaxToken Peek(int offset)
        {
            if (_position + offset >= _tokens.Length)
                return _tokens[_tokens.Length - 1];

            return _tokens[_position + offset];
        }

        private SyntaxToken Current => Peek(0);

        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }

        private SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            return new SyntaxToken(kind, Current.Position, null, null); //??
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            var numberToken = Match(SyntaxKind.NumberToken);
            return new NumberExperssionSyntax(numberToken);
        }

        public  ExpressionSyntax Parse()
        {
            var left = ParsePrimaryExpression();

            while(Current.Kind == SyntaxKind.PlusToken || Current.Kind == SyntaxKind.MinusToken)
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }


    }

}
