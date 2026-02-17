using System.Text.RegularExpressions;

namespace DfaSim;

internal static partial class Program
{
    public static void Main(string[] args)
    {
        Graph graph;
        if (args.Length > 0) graph = ParseDfa(args);
        else graph = ParseDfa(ReadRules());

        static IEnumerable<string> ReadRules()
        {
            Console.WriteLine("List the rules for this DFA. Type \"end\" to complete.\n");

            string? input;
            while (!string.IsNullOrWhiteSpace(input = Console.ReadLine()) && !input.Equals("end"))
            {
                yield return input;
            }
            Console.Clear();
        }

        // Print some stats.
        Console.WriteLine($"Graph has {graph.States.Count} states");
        Console.WriteLine($"Alphabet is \"{graph.Alphabet()}\"");
        Console.WriteLine($"\nMermaid is:\n{graph.FlowchartSyntax()}");

        Console.WriteLine("\n\n");
        while (true)
        {
            string str = Console.ReadLine()!;
            if (graph.Test(str)) Console.WriteLine("Pass");
            else Console.WriteLine("Fail");
        }
    }

    private static Graph ParseDfa(IEnumerable<string> rules)
    {
        Graph graph = new();
        int i = 0;
        foreach (string rule in rules)
        {
            // Parse a single rule.
            i++;
            Match match;
            if ((match = EntryRegex().Match(rule)).Success)
            {
                if (graph.Entry != -1)
                {
                    Console.WriteLine($"Rule {i}: Exactly one entry state must exist.");
                    continue;
                }
                State state = graph.GetOrCreate(int.Parse(match.Groups[1].Value));
                graph.Entry = state.Id;
            }
            else if ((match = TransitionRegex().Match(rule)).Success)
            {
                State leftState = graph.GetOrCreate(int.Parse(match.Groups[1].Value));
                State rightState = graph.GetOrCreate(int.Parse(match.Groups[3].Value));

                for (int j = 0; j < match.Groups[2].Captures.Count; j++)
                {
                    char c = match.Groups[2].Captures[j].Value[0];
                    if (leftState.Transitions.TryGetValue(c, out List<int>? outs))
                        outs.Add(rightState.Id);
                    else leftState.Transitions.Add(c, [rightState.Id]);
                }
            }
            else if ((match = AcceptRegex().Match(rule)).Success)
            {
                State state = graph.GetOrCreate(int.Parse(match.Groups[1].Value));
                state.Accept = true;
            }
            else
            {
                // All regexes failed to parse.
                Console.WriteLine($"Rule {i}: Invalid");
                continue;
            }
        }
        return graph;
    }

    [GeneratedRegex(@"^enter q([0-9]+)$")] private static partial Regex EntryRegex();
    [GeneratedRegex(@"^q([0-9]+) -- (?:(.),? )+--> q([0-9]+)$")] private static partial Regex TransitionRegex();
    [GeneratedRegex(@"^accept q([0-9]+)$")] private static partial Regex AcceptRegex();
}
