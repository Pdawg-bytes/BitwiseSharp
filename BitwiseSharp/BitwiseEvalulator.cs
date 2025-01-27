global using ArbitraryNumber = System.Numerics.BigInteger;

using BitwiseSharp.Core;
using BitwiseSharp.Types;

namespace BitwiseSharp
{
    public class BitwiseEvalulator
    {
        private readonly Tokenizer _tokenizer;
        private readonly Parser _parser;
        private readonly Evaluator _evaluator;

        public readonly EnvironmentContext EnvironmentContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="BitwiseEvalulator"/> class.
        /// </summary>
        /// <param name="logCtx">Defines the behavior of verbose logs.</param>
        /// <param name="environmentContext">The context containing the current symbol table.</param>
        public BitwiseEvalulator(VerboseLogContext logCtx, EnvironmentContext environmentContext)
        {
            _tokenizer = new(logCtx);
            _parser = new(logCtx);
            _evaluator = new(environmentContext);
            EnvironmentContext = environmentContext;
        }

        /// <summary>
        /// Evaluates the <paramref name="expression"/> as a(n) <see cref="ArbitraryNumber"/>.
        /// </summary>
        /// <param name="expression">The expression to evaluate</param>
        /// <returns>A <see cref="Result{ArbitraryNumber}"/> containing the evaluated <see cref="ArbitraryNumber"/>, or a failure message if evaluation fails.</returns>
        public Result<ArbitraryNumber> EvaluateExpression(string expression)
        {
            Result<List<Token>> tokenResult = _tokenizer.Tokenize(expression);
            if (!tokenResult.IsSuccess)
            {
                return Result<ArbitraryNumber>.Failure(tokenResult.Error);
            }

            _parser.SetTokens(tokenResult.Value);

            Result<Node> parseResult = _parser.ParseExpression();
            if (!parseResult.IsSuccess)
            {
                return Result<ArbitraryNumber>.Failure(parseResult.Error);
            }

            return _evaluator.Evaluate(parseResult.Value);
        }
    }
}