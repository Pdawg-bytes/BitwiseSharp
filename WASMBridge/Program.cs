using BitwiseSharp;
using BitwiseSharp.Types;
using System.Runtime.InteropServices.JavaScript;

Console.WriteLine("Bitwise evaluator initialized.");

public partial class Program
{
    static BitwiseEvalulator _eval;

    [JSExport]
    static void Init()
    {
        VerboseLogContext logCtx = new(false, false);
        _eval = new(logCtx, new BitwiseSharp.Core.EnvironmentContext());
    }

    [JSExport]
    public static string Evalate(string expressionInput)
    {
        var exp = _eval.EvaluateExpression(expressionInput);
        if (exp.IsSuccess) return (exp.Value.ToString());
        else return $"[ERROR]: {exp.Error}";
    }
}