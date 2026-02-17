using System.Text;

namespace DfaSim;

public class Graph
{
    public int Entry { get; set; } = -1;
    public Dictionary<int, State> States { get; } = [];
    public bool EnforceDfa { get; } = true; // TODO
    
    public string Alphabet()
    {
        char[] lang = [.. (from s in States.Values
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
        foreach (State state in States.Values) result.Append($"q{state.Id} ");

        // Write the entry transition.
        if (Entry != -1) result.AppendLine().Append(indent).Append($"[*] --> q{Entry}");

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
        foreach (State state in States.Values.Where(x => x.Accept))
        {
            result.AppendLine().Append(indent).Append($"q{state.Id} --> [*]");
        }

        return result.AppendLine().ToString();
    }
    public string FlowchartSyntax()
    {
        const string indent = "    ";
        StringBuilder result = new("flowchart TD");

        // First write all states.
        result.AppendLine().Append(indent);
        for (int i = 0; i < States.Count; i++)
        {
            State state = States[i];
            result.Append($"q{state.Id}");
            if (state.Accept) result.Append($"(((q{state.Id})))");
            else result.Append($"((q{state.Id}))");

            if (i < States.Count - 1) result.Append("; ");
        }

        // Write the entry transition.
        if (Entry != -1) result.AppendLine().Append(indent).Append($"e[Entry] --> q{Entry}");

        // Now write all the other transitions.
        foreach ((int start, IEnumerable<char> cs, int end) in GetTransitions())
        {
            result.AppendLine().Append(indent).Append($"q{start} ");
            bool first = true;
            foreach (char c in cs)
            {
                if (first) result.Append("-- ");
                else result.Append(", ");
                result.Append(c);
                first = false;
            }
            result.Append($" --> q{end}");
        }

        return result.AppendLine().ToString();
    }

    public IEnumerable<(int start, IEnumerable<char> cs, int end)> GetTransitions()
    {
        foreach (State s in States.Values)
        {
            // Group transitions if the start and end are the same.
            Dictionary<int, List<char>> toEnds = [];
            foreach (KeyValuePair<char, List<int>> ts in s.Transitions)
            {
                foreach (int t in ts.Value)
                {
                    if (toEnds.TryGetValue(t, out List<char>? chars)) chars.Add(ts.Key);
                    else toEnds.Add(t, [ts.Key]);
                }
            }

            // Return 'em
            foreach (KeyValuePair<int, List<char>> tEnd in toEnds)
            {
                yield return (s.Id, tEnd.Value, tEnd.Key);
            }
        }
    }

    // A non-descriptive simulation of the DFA.
    // For a full interactive simulation, that'll come later.
    public bool Test(string str)
    {
        if (Entry == -1) return false;

        Queue<State> branches = [];
        branches.Enqueue(States[Entry]);
        
        for (int i = 0; i < str.Length; i++)
        {
            if (EnforceDfa && branches.Count != 1)
            {
                Console.WriteLine($"More than one active branch! This is likely a bug, it should have been caught elsewhere.");
                return false;
            }

            char c = str[i];
            int branchCount = branches.Count;
            for (int j = 0; j < branchCount; j++)
            {
                State state = branches.Dequeue();

                // Try and map a new state, if there
                // exists a transition to one.
                if (state.Transitions.TryGetValue(c, out List<int>? newStates))
                {
                    if (EnforceDfa && newStates.Count > 1)
                    {
                        Console.WriteLine($"State {state} has more than one transition function for {c}!");
                        return false;
                    }
                    foreach (int newState in newStates) branches.Enqueue(States[newState]);
                }
                else if (EnforceDfa)
                {
                    Console.WriteLine($"State {state} has no transition function for {c}!");
                    return false;
                }
            }
        }

        // We need at least one state we're in to be an accept state.
        while (branches.Count > 0)
        {
            State endState = branches.Dequeue();
            if (endState.Accept) return true;
        }
        return false;
    }

    public State GetOrCreate(int id)
    {
        if (States.TryGetValue(id, out State? state)) return state;
        else
        {
            state = new(id);
            States.Add(id, state);
            return state;
        }
    }

    public bool TransitionStartsWith(char transition, int startId)
    {
        if (States.TryGetValue(startId, out State? start)) return start.Transitions.ContainsKey(transition);
        else return false;
    }
    public bool TransitionEndsWith(char transition, int endId)
    {
        foreach (State s in States.Values)
        {
            if (s.Transitions.TryGetValue(transition, out List<int>? possibleEnds) &&
                possibleEnds.Contains(endId)) return true;
        }
        return false;
    }
}
