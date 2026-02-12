using System.Text;

namespace DfaSim;

public class Graph
{
    public int Entry { get; set; } = -1;
    public List<State> States { get; } = [];
    
    public string Alphabet()
    {
        char[] lang = [.. (from s in States
                       from kvp in s.Transitions
                       select kvp.Key).Distinct()];
        Array.Sort(lang);
        return new(lang);
    }
    public string MermaidSyntax()
    {
        const string indent = "    ";
        StringBuilder result = new("stateDiagram-v2");

        // First write all states.
        result.AppendLine().Append(indent);
        foreach (State state in States) result.Append($"q{state.Id} ");

        // Write the entry transition.
        if (Entry != -1) result.AppendLine().Append(indent).Append($"[*] --> q{States[Entry].Id}");

        // Now write all the other transitions.
        foreach ((int start, IEnumerable<char> cs, int end) in GetTransitions())
        {
            result.AppendLine().Append(indent).Append($"q{start} --> q{end}");
            bool first = true;
            foreach (char c in cs)
            {
                if (first) result.Append(": ");
                else result.Append(", ");
                result.Append(c);
                first = false;
            }
        }

        // Now write accept states.
        foreach (State state in States.Where(x => x.Accept))
        {
            result.AppendLine().Append(indent).Append($"q{state.Id} --> [*]");
        }

        return result.AppendLine().ToString();
    }

    public IEnumerable<(int start, IEnumerable<char> cs, int end)> GetTransitions()
    {
        foreach (State s in States)
        {
            // Group transitions if the start and end are the same.
            Dictionary<int, List<char>> toEnds = [];
            foreach (KeyValuePair<char, int> t in s.Transitions)
            {
                if (toEnds.TryGetValue(t.Value, out List<char>? chars)) chars.Add(t.Key);
                else toEnds.Add(t.Value, [t.Key]);
            }

            // Return 'em
            foreach (KeyValuePair<int, List<char>> tEnd in toEnds)
            {
                yield return (s.Id, tEnd.Value, tEnd.Key);
            }
        }
    }

    public void SortStates() => States.Sort((a, b) => a.Id.CompareTo(b.Id));

    public State GetOrCreate(int id)
    {
        State? possible = States.SingleOrDefault(x => x.Id == id);
        if (possible is not null) return possible;

        possible = new(id);
        States.Add(possible);
        return possible;
    }

    public bool TransitionStartsWith(char transition, int startId)
    {
        State? s = States.SingleOrDefault(x => x.Id == startId);
        if (s is null) return false;
        else return s.Transitions.ContainsKey(transition);
    }
    public bool TransitionEndsWith(char transition, int endId)
    {
        foreach (State s in States)
        {
            if (s.Transitions.TryGetValue(transition, out int possibleEnd) &&
                possibleEnd == endId) return true;
        }
        return false;
    }
}
