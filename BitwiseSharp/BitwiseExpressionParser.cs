global using ArbitraryNumber = System.Numerics.BigInteger;

using BitwiseSharp.Core;
using BitwiseSharp.Types;

namespace BitwiseSharp
{
    public class BitwiseExpressionParser
    {
        private readonly bool _verbose;
        private readonly Parser _parser;
        private readonly Evaluator _evaluator;

        public BitwiseExpressionParser(bool verbose)
        {
            _verbose = verbose;

            Tokenizer.Verbose = verbose;
            _parser = new(verbose);
            _evaluator = new(verbose);
        }

        public ArbitraryNumber EvaluateExpression(string expression)
        {
            List<Token> tokens = Tokenizer.Tokenize(expression);
            _parser.SetTokens(tokens);
            Node parsed = _parser.Parse();
            return _evaluator.Evaluate(parsed);
        }
    }
}