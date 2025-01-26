using BitwiseSharp.Types;
using BitwiseSharp.Enums;

namespace BitwiseSharp.Core
{
    internal class Evaluator
    {
        private readonly bool _verbose;

        internal Evaluator(bool verbose) 
        {
            _verbose = verbose;
        }

        internal ArbitraryNumber Evaluate(Node node)
        {
            switch (node)
            {
                case NumberNode numberNode:
                    return numberNode.Value;
                
                case UnaryNode unaryNode:
                    ArbitraryNumber operand = Evaluate(unaryNode.Operand);
                    return unaryNode.Operator switch
                    {
                        TokenType.BitwiseNot => ~operand,
                        _ => throw new InvalidOperationException($"Invalid unary operator type: {unaryNode.Operator}")
                    };

                case BinaryNode binaryNode:
                    ArbitraryNumber left = Evaluate(binaryNode.Left);
                    ArbitraryNumber right = Evaluate(binaryNode.Right);
                    return binaryNode.Operator switch
                    {
                        TokenType.BitwiseAnd => left & right,
                        TokenType.BitwiseOr => left | right,
                        TokenType.BitwiseXor => left ^ right,
                        TokenType.LeftShift => left << (int)right,
                        TokenType.RightShift => left >> (int)right,
                        _ => throw new InvalidOperationException($"Invalid binary operator type: {binaryNode.Operator}")
                    };
            }

            return 0;
        }
    }
}
