using BitwiseSharp;

namespace InteractiveShell
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BitwiseExpressionParser parser = new(true);

            Console.Write("Enter a bitwise expression to evaluate: ");
            string expr = Console.ReadLine();
            Console.WriteLine(parser.EvaluateExpression(expr));
        }
    }
}
