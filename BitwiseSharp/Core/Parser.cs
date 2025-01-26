using BitwiseSharp.Enums;
using BitwiseSharp.Types;

namespace BitwiseSharp.Core
{
    internal class Parser
    {
        private List<Token> _tokens;
        private int _currentTokenIndex;

        private bool _verbose;

        internal Parser(bool verbose)
        {
            _verbose = verbose;
        }

        internal void SetTokens(List<Token> tokens)
        {
            _tokens = tokens;
            _currentTokenIndex = 0;
        }

        internal Node Parse()
        {
            Node parsed = ParseExpression();
            if (_verbose) Console.WriteLine("Expression AST: \n" + PrintAST(parsed));
            return parsed;
        }

        private Node ParseExpression(int minPrecedence = -1)
        {
            Node left = ParsePrimary();

            while (_currentTokenIndex < _tokens.Count)
            {
                Token currentToken = _tokens[_currentTokenIndex];
                int precedence = Precedence.GetPrecedence(currentToken.Type);

                if (precedence < minPrecedence)
                    break;

                _currentTokenIndex++;
                Node right = ParsePrimary();

                while (_currentTokenIndex < _tokens.Count)
                {
                    Token nextToken = _tokens[_currentTokenIndex];
                    int nextPrecedence = Precedence.GetPrecedence(nextToken.Type);

                    if (nextPrecedence <= precedence)
                        break;

                    right = ParseExpression(precedence + 1);
                }

                left = currentToken.Type switch
                {
                    TokenType.BitwiseAnd => new BinaryNode("&", TokenType.BitwiseAnd, left, right),
                    TokenType.BitwiseOr => new BinaryNode("|", TokenType.BitwiseOr, left, right),
                    TokenType.BitwiseXor => new BinaryNode("^", TokenType.BitwiseXor, left, right),
                    TokenType.LeftShift => new BinaryNode("<<", TokenType.LeftShift, left, right),
                    TokenType.RightShift => new BinaryNode(">>", TokenType.RightShift, left, right),
                    _ => left
                };

                Console.WriteLine("Expression AST: " + $"current token: {_currentTokenIndex}" + "\n" + PrintAST(left));
            }

            
            return left;
        }

        private Node ParsePrimary()
        {
            Token currentToken = _tokens[_currentTokenIndex];
            _currentTokenIndex++;

            switch (currentToken.Type)
            {
                case TokenType.Number:
                    return new NumberNode(currentToken.TokenValue);

                case TokenType.LeftParenthesis:
                    Node expr = ParseExpression();
                    if (_tokens[_currentTokenIndex].Type != TokenType.RightParenthesis)
                        throw new Exception("Expected closing parenthesis.");
                    _currentTokenIndex++;
                    return expr;

                case TokenType.BitwiseNot:
                    return new UnaryNode("~", TokenType.BitwiseNot, ParsePrimary());
            }

            throw new Exception($"Unexpected token: {currentToken.Type} value {currentToken.TokenValue} at token {_currentTokenIndex}");
        }

        private static string PrintAST(Node node, int indentLevel = 0)
        {
            string indent = new string(' ', indentLevel * 4);

            return node switch
            {
                NumberNode numberNode =>
                    $"{indent}Number: {numberNode.Value}\n",

                UnaryNode unaryNode =>
                    $"{indent}Unary: {unaryNode.Operator}\n" +
                    $"{PrintAST(unaryNode.Operand, indentLevel + 1)}",

                BinaryNode binaryNode =>
                    $"{indent}Binary: {binaryNode.Operator}\n" +
                    $"{PrintAST(binaryNode.Left, indentLevel + 1)}" +
                    $"{PrintAST(binaryNode.Right, indentLevel + 1)}",

                _ => $"{indent}Unknown node\n"
            };
        }
    }
}
