using BitwiseSharp.Enums;
using BitwiseSharp.Types;

namespace BitwiseSharp.Core
{
    internal class Parser
    {
        private List<Token> _tokens;
        private int _position;

        private bool _verbose;

        internal Parser(bool verbose)
        {
            _verbose = verbose;
        }

        internal void SetTokens(List<Token> tokens)
        {
            _tokens = tokens;
            _position = 0;
        }

        internal Result<Node> ParseExpression()
        {
            if (_tokens[0].Type == TokenType.Let)
            {
                if (_tokens[1].Type != TokenType.Identifier)
                    return Result<Node>.Failure("Invalid syntax: Expected a variable name after 'let'.");

                string variableName = _tokens[1].VariableName;
                if (_tokens[2].Type != TokenType.Assignment)
                    return Result<Node>.Failure("Invalid syntax: Expected '=' after the variable name.");

                _tokens = _tokens.Skip(3).ToList();
                var binaryResult = ParseBinaryExpression(0);
                if (!binaryResult.IsSuccess) return binaryResult;

                return Result<Node>.Success(new VariableDefinitionNode(variableName, binaryResult.Value));
            }

            if (_tokens.Count > 1 && (_tokens[0].Type == TokenType.Identifier && _tokens[1].Type == TokenType.Assignment))
            {
                string variableName = _tokens[0].VariableName;
                if (_tokens[1].Type != TokenType.Assignment)
                    return Result<Node>.Failure("Invalid syntax: Expected '=' after the variable name.");

                _tokens = _tokens.Skip(2).ToList();
                var binaryResult = ParseBinaryExpression(0);
                if (!binaryResult.IsSuccess) return binaryResult;

                return Result<Node>.Success(new VariableAssignmentNode(variableName, binaryResult.Value));
            }

            return ParseBinaryExpression(0);
        }

        private Result<Node> ParseBinaryExpression(int minPrecedence)
        {
            var leftResult = ParseUnaryExpression();
            if (!leftResult.IsSuccess) return leftResult;

            Node left = leftResult.Value;

            while (_position < _tokens.Count && IsOperator(CurrentToken()))
            {
                var (precedence, rightAssociative) = Precedence.OperatorPrecedence[CurrentToken().Type];
                if (precedence < minPrecedence) break;

                Token op = ConsumeToken();

                int nextMinPrecedence = rightAssociative ? precedence : precedence + 1;
                var rightResult = ParseBinaryExpression(nextMinPrecedence);
                if (!rightResult.IsSuccess) return rightResult;

                Node right = rightResult.Value;
                left = new BinaryNode(op.Type, left, right);
            }

            return Result<Node>.Success(left);
        }

        private Result<Node> ParseUnaryExpression()
        {
            if (IsUnaryOperator(CurrentToken()))
            {
                Token op = ConsumeToken();
                var operandResult = ParseUnaryExpression();
                if (!operandResult.IsSuccess) return operandResult;

                return Result<Node>.Success(new UnaryNode(op.Type, operandResult.Value));
            }

            return ParsePrimary();
        }

        private Result<Node> ParsePrimary()
        {
            if (CurrentToken().Type == TokenType.Number)
            {
                Token numberToken = ConsumeToken();
                return Result<Node>.Success(new NumberNode(numberToken.NumberValue));
            }

            if (CurrentToken().Type == TokenType.LeftParenthesis)
            {
                ConsumeToken();
                var exprResult = ParseExpression();
                if (!exprResult.IsSuccess)return exprResult;

                if (CurrentToken().Type != TokenType.RightParenthesis)
                    return Result<Node>.Failure("Expected closing parenthesis");

                ConsumeToken();
                return Result<Node>.Success(exprResult.Value);
            }

            if (CurrentToken().Type == TokenType.Identifier)
            {
                string variableName = CurrentToken().VariableName;
                ConsumeToken();
                return Result<Node>.Success(new VariableReferenceNode(variableName));
            }

            return Result<Node>.Failure($"Unexpected token: {CurrentToken().Type}");
        }

        private Token CurrentToken() => _position < _tokens.Count ? _tokens[_position] : null;
        private Token ConsumeToken() => _tokens[_position++];
        private bool IsOperator(Token token) => Precedence.OperatorPrecedence.ContainsKey(token.Type);
        private bool IsUnaryOperator(Token token) => token.Type == TokenType.BitwiseNot;
    }
}
