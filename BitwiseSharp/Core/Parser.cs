using BitwiseSharp.Enums;
using BitwiseSharp.Types;

using static BitwiseSharp.Constants.Colors;

namespace BitwiseSharp.Core
{
    /// <summary>
    /// Parses a list of <see cref="Token"/>s using a recursive descent parser to construct an AST.
    /// </summary>
    internal class Parser
    {
        private List<Token> _tokens;
        private int _position;

        private readonly VerboseLogContext _logCtx;

        /// <summary>Initializes an instance of the <see cref="Parser"/> class.</summary>
        /// <param name="logCtx">The logging context of the parser.</param>
        internal Parser(VerboseLogContext logCtx) => _logCtx = logCtx;

        /// <summary>
        /// Sets the tokens to be parsed in the current context.
        /// </summary>
        /// <param name="tokens">The list of tokens to be set.</param>
        internal void SetTokens(List<Token> tokens)
        {
            _tokens = tokens;
            _position = 0;
        }

        /// <summary>
        /// Parses the entire expression, starting from the root of the expression tree.
        /// </summary>
        /// <returns>A <see cref="Result{Node}"/> containing the root <see cref="Node"/> of the parsed expression, or a failure message if parsing fails.</returns>
        internal Result<Node> ParseExpression()
        {
            Result<Node> result = ParseRoot();

            if (result.IsSuccess && _logCtx.Verbose)
                _logCtx.Log(VerboseLogType.Parser, "\n" + PrintAST(result.Value));

            return result;
        }

        /// <summary>
        /// Parses the root of the expression, which can be a variable definition, assignment, or a binary expression.
        /// </summary>
        /// <returns>A <see cref="Result{Node}"/> containing the parsed <see cref="Node"/>, or a failure message if parsing fails.</returns>
        // Non-bitwise/arithmetic expressions are caught here.
        private Result<Node> ParseRoot()
        {
            if (_tokens == null || _tokens.Count == 0)
                return Result<Node>.Failure("Unexpected end of input while parsing expression.");

            // Handle variable definition (`let`)
            if (_tokens[0].Type == TokenType.Let)
            {
                if (_tokens.Count < 3 || _tokens[1].Type != TokenType.Identifier)
                    return Result<Node>.Failure("Invalid syntax: Expected a variable name after 'let'.");

                string variableName = _tokens[1].VariableName;
                if (_tokens[2].Type != TokenType.Assignment)
                    return Result<Node>.Failure("Invalid syntax: Expected '=' after the variable name.");

                _tokens = _tokens.Skip(3).ToList();
                var binaryResult = ParseBinaryExpression(0);
                if (!binaryResult.IsSuccess) return binaryResult;

                return Result<Node>.Success(new VariableDefinitionNode(variableName, binaryResult.Value));
            }

            // Handle variable assignment
            if (_tokens.Count > 1 && _tokens[0].Type == TokenType.Identifier && _tokens[1].Type == TokenType.Assignment)
            {
                string variableName = _tokens[0].VariableName;

                _tokens = _tokens.Skip(2).ToList();
                var binaryResult = ParseBinaryExpression(0);
                if (!binaryResult.IsSuccess) return binaryResult;

                return Result<Node>.Success(new VariableAssignmentNode(variableName, binaryResult.Value));
            }

            // Handle assignment with operators
            if (_tokens.Count > 1 && _tokens[0].Type == TokenType.Identifier && Precedence.IsOperator(_tokens[1].Type) && _tokens[2].Type == TokenType.Assignment)
            {
                string variableName = _tokens[0].VariableName;

                _tokens.RemoveAt(2);
                _tokens.Insert(2, new Token(TokenType.LeftParenthesis));
                _tokens.Add(new Token(TokenType.RightParenthesis));

                var binaryResult = ParseBinaryExpression(0);
                if (!binaryResult.IsSuccess) return binaryResult;

                return Result<Node>.Success(new VariableAssignmentNode(variableName, binaryResult.Value));
            }

            // Handle invalid assignment to a number
            if (_tokens[_tokens.Count - 1].Type == TokenType.Identifier && _tokens[_tokens.Count - 2].Type == TokenType.Assignment)
                return Result<Node>.Failure("Invalid syntax: Left-hand side of assignment must be a variable.");

            return ParseBinaryExpression(0);
        }


        /// <summary>
        /// Parses a binary expression, which can have multiple operators with varying precedence.
        /// </summary>
        /// <param name="minPrecedence">The minimum precedence of operators to parse.</param>
        /// <returns>A <see cref="Result{Node}"/> containing the parsed <see cref="Node"/>, or a failure message if parsing fails.</returns>
        private Result<Node> ParseBinaryExpression(int minPrecedence)
        {
            var leftResult = ParseUnaryExpression();
            if (!leftResult.IsSuccess) return leftResult;

            Node left = leftResult.Value;

            while (_position < _tokens.Count)
            {
                var currentTokenResult = CurrentToken();
                if (!currentTokenResult.IsSuccess) return Result<Node>.Failure(currentTokenResult.Error);

                var currentToken = currentTokenResult.Value;

                if (!Precedence.IsOperator(currentToken.Type)) break;

                var (precedence, rightAssociative) = Precedence.OperatorPrecedence[currentToken.Type];
                if (precedence < minPrecedence) break;

                var consumeTokenResult = ConsumeToken();
                if (!currentTokenResult.IsSuccess) return Result<Node>.Failure(currentTokenResult.Error);

                var op = consumeTokenResult.Value;

                int nextMinPrecedence = rightAssociative ? precedence : precedence + 1;
                Result<Node> rightResult = ParseBinaryExpression(nextMinPrecedence);
                if (!rightResult.IsSuccess) return rightResult;

                Node right = rightResult.Value;
                left = new BinaryNode(op.Type, left, right);
            }

            return Result<Node>.Success(left);
        }

        /// <summary>
        /// Parses a unary expression, which can be preceded by unary operators such as negation or bitwise NOT.
        /// </summary>
        /// <returns>A <see cref="Result{Node}"/> containing the parsed <see cref="Node"/>, or a failure message if parsing fails.</returns>
        private Result<Node> ParseUnaryExpression()
        {
            if (_position >= _tokens.Count)
                return Result<Node>.Failure("Unexpected end of input while parsing unary expression.");

            var currentTokenResult = CurrentToken();
            if (!currentTokenResult.IsSuccess) return Result<Node>.Failure(currentTokenResult.Error);

            var currentToken = currentTokenResult.Value;

            if (IsUnaryOperator(currentToken))
            {
                var consumeTokenResult = ConsumeToken();
                if (!currentTokenResult.IsSuccess) return Result<Node>.Failure(currentTokenResult.Error);

                var op = consumeTokenResult.Value;
                var operandResult = ParseUnaryExpression();
                if (!operandResult.IsSuccess) return operandResult;

                return Result<Node>.Success(new UnaryNode(op.Type, operandResult.Value));
            }

            return ParsePrimary();
        }

        /// <summary>
        /// Parses a primary expression, which could be a number, an identifier, or an expression inside parentheses.
        /// </summary>
        /// <returns>A <see cref="Result{Node}"/> containing the parsed <see cref="Node"/>, or a failure message if parsing fails.</returns>
        private Result<Node> ParsePrimary()
        {
            if (_position >= _tokens.Count)
                return Result<Node>.Failure("Unexpected end of input while parsing primary expression.");

            var currentTokenResult = CurrentToken();
            if (!currentTokenResult.IsSuccess) return Result<Node>.Failure(currentTokenResult.Error);

            var currentToken = currentTokenResult.Value;

            if (currentToken.Type == TokenType.Number)
            {
                var consumeTokenResult = ConsumeToken();
                if (!currentTokenResult.IsSuccess) return Result<Node>.Failure(currentTokenResult.Error);

                Token numberToken = consumeTokenResult.Value;
                return Result<Node>.Success(new NumberNode(numberToken.NumberValue));
            }

            if (currentToken.Type == TokenType.LeftParenthesis)
            {
                var consumeTokenResult = ConsumeToken();
                if (!currentTokenResult.IsSuccess) return Result<Node>.Failure(currentTokenResult.Error);

                var exprResult = ParseRoot();
                if (!exprResult.IsSuccess) return exprResult;

                var closingParenthesisResult = CurrentToken();
                if (!closingParenthesisResult.IsSuccess || closingParenthesisResult.Value.Type != TokenType.RightParenthesis)
                    return Result<Node>.Failure("Invalid syntax: Expected closing parenthesis.");

                var consumeClosingResult = ConsumeToken();
                if (!consumeClosingResult.IsSuccess) return Result<Node>.Failure(consumeClosingResult.Error);

                return Result<Node>.Success(exprResult.Value);
            }

            if (currentToken.Type == TokenType.Identifier)
            {
                string variableName = currentToken.VariableName;
                var consumeTokenResult = ConsumeToken();
                if (!currentTokenResult.IsSuccess) return Result<Node>.Failure(currentTokenResult.Error);

                return Result<Node>.Success(new VariableReferenceNode(variableName));
            }

            return Result<Node>.Failure($"Invalid syntax: Unexpected token: {currentToken.Type}");
        }

        /// <summary>
        /// Retrieves the current token in the list, if available.
        /// </summary>
        /// <returns>A <see cref="Result{Token}"/> containing the current <see cref="Token"/>, or a failure message if there is no current token.</returns>
        private Result<Token> CurrentToken()
        {
            if (_position < _tokens.Count)
                return Result<Token>.Success(_tokens[_position]);
            else
                return Result<Token>.Failure("Unexpected end of input while parsing.");
        }
        /// <summary>
        /// Consumes the current token and moves to the next token in the list.
        /// </summary>
        /// <returns>A <see cref="Result{Token}"/> containing the consumed <see cref="Token"/>, or a failure message if no token is available.</returns>
        private Result<Token> ConsumeToken()
        {
            if (_position < _tokens.Count) return Result<Token>.Success(_tokens[_position++]);
            else
                return Result<Token>.Failure("Unexpected end of input while consuming token.");
        }

        /// <summary>
        /// Determines whether the given token is a unary operator.
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <returns><c>true</c> if the token is a unary operator; otherwise, <c>false</c>.</returns>
        private bool IsUnaryOperator(Token token)
        {
            if (!(token.Type == TokenType.BitwiseNot || token.Type == TokenType.Minus)) return false;

            return _position == 0 ||
                Precedence.IsOperator(_tokens[_position - 1].Type) ||
                _tokens[_position - 1].Type == TokenType.LeftParenthesis;
        }


        /// <summary>
        /// Prints the abstract syntax tree starting from a given <see cref="Node"/>.
        /// </summary>
        /// <param name="node">The root <see cref="Node"/>.</param>
        /// <param name="indentLevel">The level at which the current <see cref="Node"/> is indented in the output.</param>
        /// <returns></returns>
        private string PrintAST(Node node, int indentLevel = 0)
        {
            string indent = new string(' ', indentLevel * 4);
            string reset = _logCtx.GetColor(ANSI_RESET);

            return node switch
            {
                NumberNode numberNode =>
                    $"{indent}{_logCtx.GetColor(ORANGE)}Number:{reset} {numberNode.Value}{reset}\n",

                UnaryNode unaryNode =>
                    $"{indent}{_logCtx.GetColor(PINK)}Unary:{reset} {_logCtx.GetColor(YELLOW)}{unaryNode.Operator}{reset}\n" +
                    $"{PrintAST(unaryNode.Operand, indentLevel + 1)}",

                BinaryNode binaryNode =>
                    $"{indent}{_logCtx.GetColor(LIGHT_PURPLE)}Binary:{reset} {_logCtx.GetColor(YELLOW)}{binaryNode.Operator}{reset}\n" +
                    $"{PrintAST(binaryNode.Left, indentLevel + 1)}" +
                    $"{PrintAST(binaryNode.Right, indentLevel + 1)}",

                VariableDefinitionNode variableDefinitionNode =>
                    $"{indent}{_logCtx.GetColor(CYAN)}Variable Definition:{reset} {variableDefinitionNode.VariableName}{reset}\n" +
                    $"{PrintAST(variableDefinitionNode.RightHandSide, indentLevel + 1)}",

                VariableAssignmentNode variableAssignmentNode =>
                    $"{indent}{_logCtx.GetColor(LIGHT_BLUE)}Variable Assignment:{reset} {variableAssignmentNode.VariableName}{reset}\n" +
                    $"{PrintAST(variableAssignmentNode.RightHandSide, indentLevel + 1)}",

                VariableReferenceNode variableReferenceNode =>
                    $"{indent}{_logCtx.GetColor(GREEN)}Variable Reference:{reset} {variableReferenceNode.VariableName}{reset}\n",

                _ => $"{indent}{_logCtx.GetColor(RED)}Unknown node{reset}\n"
            };
        }
    }
}