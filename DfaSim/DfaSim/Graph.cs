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
