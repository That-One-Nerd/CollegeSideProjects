using PropositionReducer.Expressions;
using System.Text;

using UnaryOperatorInfo = (string[] aliases, System.Func<PropositionReducer.Expressions.Expression, PropositionReducer.Expressions.Expression> constructor);
using BinaryOperatorInfo = (string[] aliases, System.Func<PropositionReducer.Expressions.Expression, PropositionReducer.Expressions.Expression, PropositionReducer.Expressions.Expression> constructor);

namespace PropositionReducer;

public static class Program
{
    public static void Main(string[] args)
    {
        List<string> expStrs = [];
        if (args.Length == 0)
        {
            Console.Write("Enter a logical expression in TeX format\n > ");
            expStrs.Add(Console.ReadLine()!);

            Console.Write("\nLeave blank to reduce, or enter a second expression to compute equivalence\n > ");
            string? temp = Console.ReadLine();
            while (!string.IsNullOrEmpty(temp))
            {
                expStrs.Add(temp);
                Console.Write("\nLeave blank to continue, or enter another expression to compute truth tables\n > ");
                temp = Console.ReadLine();
            }
        }
        else expStrs = [.. args];

        int index;
        List<char> vars = [];
        Expression[] exps = new Expression[expStrs.Count];
        for (int i = 0; i < exps.Length; i++)
        {
            index = 0;
            exps[i] = ParseExpression(expStrs[i], ref index, false, ref vars);
        }

        vars.Sort();
        InputArray inputs = new([.. vars]);

        Console.OutputEncoding = Encoding.Unicode;
        Console.WriteLine($"\nExpressions:");
        for (int i = 0; i < exps.Length; i++) Console.WriteLine(exps[i]);
        Console.WriteLine($"\nWith Variables:\n{inputs.NamesToString()}");

        Console.WriteLine("\n---\n");

        if (exps.Length == 1) ReduceExpression(exps[0], inputs);
        else if (exps.Length == 2) CheckExpressionEquivalence(exps[0], exps[1], inputs);
        else PrintTruthTables(exps, inputs);
    }

    public static void ReduceExpression(Expression exp, InputArray inputs)
    {
        Console.WriteLine($"Expression Truth Table:\n{GetTruthTable([exp], inputs, false)}");

        // TODO
    }
    public static void CheckExpressionEquivalence(Expression exp1, Expression exp2, InputArray inputs)
    {
        // Compute equivalence.
        bool equivalent = true;
        int combos = 1 << inputs.Count;
        for (int c = 0; c < combos; c++)
        {
            // Set input values.
            for (int i = 0, mask = 1; mask < combos; i++, mask <<= 1)
            {
                bool val = (c & mask) > 0;
                inputs[i] = val;
            }

            // Compute expressions.
            if (exp1.Evaluate(inputs) != exp2.Evaluate(inputs))
            {
                equivalent = false;
                break;
            }
        }

        if (equivalent) Console.WriteLine("The two expressions are \x1b[3;32mEQUIVALENT\x1b[0m.");
        else Console.WriteLine("The two expressions are \x1b[3;31mNOT EQUIVALENT\x1b[0m.");
        Console.WriteLine(GetTruthTable([exp1, exp2], inputs, true));
    }
    public static void PrintTruthTables(Expression[] exps, InputArray inputs)
    {
        Console.WriteLine($"Truth tables for the given expressions:");
        Console.WriteLine(GetTruthTable(exps, inputs, false));
    }

    private static string GetTruthTable(Expression[] exps, InputArray inputs, bool printDifference)
    {
        const string True = "\x1b[42m T \x1b[0m",
                     False = " F ",
                     UnequalTrue = "\x1b[30;43m T \x1b[0m",
                     UnequalFalse = "\x1b[33m F \x1b[0m";

        StringBuilder truth = new("|");
        for (int i = 0; i < inputs.Count; i++) truth.Append($" {inputs.GetName(i)} |");
        if (exps.Length == 1) truth.Append(" Result |\n|");
        else
        {
            for (int i = 0; i < exps.Length; i++) truth.Append($" Expr.{i} |");
            truth.Append("\n|");
        }
        for (int i = 0; i < inputs.Count; i++) truth.Append("---|");

        // Slightly breaks at 10+ expressions. But who would make that many??
        for (int i = 0; i < exps.Length; i++) truth.Append("--------|");
        truth.AppendLine();

        int combos = 1 << inputs.Count;
        for (int c = 0; c < combos; c++)
        {
            truth.Append('|');

            // Set input values and print input for truth table.
            for (int i = 0, mask = 1; mask < combos; i++, mask <<= 1)
            {
                bool val = (c & mask) > 0;
                inputs[i] = val;
                truth.Append($"{(val ? True : False)}|");
            }

            // Compute and print expression results.
            bool last = false;
            for (int e = 0; e < exps.Length; e++)
            {
                bool result = exps[e].Evaluate(inputs);
                if (e == 0) last = result;
                truth.Append($"    {((result == last || !printDifference) ? (result ? True : False) : (result ? UnequalTrue : UnequalFalse))} |");
            }
            truth.AppendLine();
        }
        return truth.ToString();
    }

    private static readonly List<UnaryOperatorInfo> unaryOperators = [
        (["neg"], (x) => new NegationOperator(x))
    ];
    private static readonly List<BinaryOperatorInfo> binaryOperators = [
        (["land", "and", "^"], (x, y) => new LogicalAndOperator(x, y)),
        (["lor", "or", "v"], (x, y) => new LogicalOrOperator(x, y)),
        (["to", "implies", "->"], (x, y) => new ImpliesOperator(x, y)),
        (["gets", "<-"], (x, y) => new ImpliesOperator(y, x)), // Tricky!
        (["leftrightarrow", "biconditional", "iff", "<->"], (x, y) => new BiConditionalOperator(x, y)),
    ];

    public static Expression ParseExpression(ReadOnlySpan<char> str, ref int index, bool inParen, ref List<char> vars)
    {
        Expression? current = null;
        while (index < str.Length)
        {
            ReadOnlySpan<char> word = ReadWord(str, ref index);
            if (word.Length == 0) continue; // Most likely EOF, but we'll keep going just in case.
            else if (word.Length == 1)
            {
                char c = word[0];

                // Parenthesis
                if (c == '(')
                {
                    if (current is not null) throw new ArgumentException("Expected operator, got parenthetical expression.");
                    current = new ParenthesisExpression(ParseExpression(str, ref index, true, ref vars));
                    continue;
                }
                else if (c == ')')
                {
                    if (!inParen) index--;
                    break;
                }

                // Variable
                if (current is not null) throw new ArgumentException($"Expected an operator, got variable expression (token {word}).");
                else if (c == 'T') current = new ConstantValue(true);
                else if (c == 'F') current = new ConstantValue(false);
                else if (!char.IsLetter(c)) throw new ArgumentException($"Variable must be a single letter (got {word}).");
                else
                {
                    current = new VariableValue(c);
                    if (!vars.Contains(c)) vars.Add(c);
                }
            }
            else if (word.StartsWith(@"\"))
            {
                // Operator
                word = word[1..];
                string wordStr = word.ToString();

                UnaryOperatorInfo unary = unaryOperators.FirstOrDefault(x => x.aliases.Any(y => y == wordStr));
                if (unary.aliases is not null)
                {
                    if (current is not null) throw new ArgumentException($@"Expected binary operator, got unary operator (operator \{word}).");
                    else current = unary.constructor(ParseExpression(str, ref index, false, ref vars));
                    continue;
                }

                BinaryOperatorInfo binary = binaryOperators.FirstOrDefault(x => x.aliases.Any(y => y == wordStr));
                if (binary.aliases is not null)
                {
                    if (current is null) throw new ArgumentException($@"Binary operator does not have a left-side expression (operator \{word}).");
                    else current = binary.constructor(current, ParseExpression(str, ref index, false, ref vars));
                    continue;
                }
                throw new ArgumentException($@"Unknown operator \{word}.");
            }
            else throw new ArgumentException($"Unknown token \"{word}\""); // I dunno.
        }

        return current ?? new ConstantValue(true);

        static ReadOnlySpan<char> ReadWord(ReadOnlySpan<char> str, ref int index)
        {
            int start = index;
            bool anything = false;
            while (index < str.Length)
            {
                char c = str[index];
                if (c == ' ')
                {
                    if (anything) break;
                    else start++;
                }
                else if (anything && c == '\\') break;
                else if (c == '(' || c == ')')
                {
                    if (!anything) index++;
                    break;
                }
                else anything = true;
                index++;
            }
            return str[start..index].Trim();
        }
    }
}
