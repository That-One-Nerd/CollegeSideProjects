using PropositionReducer.Expressions;
using System.Text;

using UnaryOperatorInfo = (string[] aliases, System.Func<PropositionReducer.Expressions.Expression, PropositionReducer.Expressions.Expression> constructor);
using BinaryOperatorInfo = (string[] aliases, System.Func<PropositionReducer.Expressions.Expression, PropositionReducer.Expressions.Expression, PropositionReducer.Expressions.Expression> constructor);

namespace PropositionReducer;

public static class Program
{
    public static void Main(string[] args)
    {
        string expStr1, expStr2;
        if (args.Length == 0)
        {
            Console.Write("Enter a logical expression in TeX format\n > ");
            expStr1 = Console.ReadLine()!;

            Console.Write("\nLeave blank to reduce, or enter a second expression to compute equivalence\n > ");
            expStr2 = Console.ReadLine()!;
        }
        else
        {
            StringBuilder sum = new();
            for (int i = 0; i < args.Length; i++) sum.Append(args[i]);
            string sumStr = sum.ToString();

            const string equivStr = @"\equiv";
            int equivPlace = sumStr.IndexOf(equivStr);
            if (equivPlace == -1)
            {
                expStr1 = sumStr;
                expStr2 = "";
            }
            else
            {
                expStr1 = sumStr[..equivPlace];
                expStr2 = sumStr[(equivPlace + equivStr.Length)..];
            }
        }

        int index = 0;
        List<char> vars = [];
        Expression exp1 = ParseExpression(expStr1, ref index, false, ref vars);
        Expression? exp2;

        if (string.IsNullOrEmpty(expStr2)) exp2 = null;
        else
        {
            index = 0;
            exp2 = ParseExpression(expStr2, ref index, false, ref vars);
        }

        InputArray inputs = new([.. vars]);
        Console.WriteLine($"\nExpressions:\n{exp1}");
        if (exp2 is not null) Console.WriteLine(exp2);
        Console.WriteLine($"\nWith Variables:\n{inputs.NamesToString()}");

        if (exp2 is null) ReduceExpression(exp1, inputs);
        else CheckExpressionEquivalence(exp1, exp2, inputs);
    }

    public static void ReduceExpression(Expression exp, InputArray inputs)
    {
        // TODO
    }
    public static void CheckExpressionEquivalence(Expression exp1, Expression exp2, InputArray inputs)
    {
        // TODO
    }

    private static readonly List<UnaryOperatorInfo> unaryOperators = [
        (["neg"], (x) => new NegationOperator(x))
    ];
    private static readonly List<BinaryOperatorInfo> binaryOperators = [
        (["land", "and", "^"], (x, y) => new LogicalAndOperator(x, y)),
        (["lor", "or", "v"], (x, y) => new LogicalOrOperator(x, y)),
        (["to", "implies", "->"], (x, y) => new ImpliesOperator(x, y)),
        (["gets", "<-"], (x, y) => new ImpliesOperator(y, x)), // Tricky!
        (["leftrightarrow", "biconditional", "bicon", "bi", "begets", "<->"], (x, y) => new BiConditionalOperator(x, y)),
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
