namespace DfaSim;

public class State(int id)
{
    public int Id { get; } = id;
    public Dictionary<char, List<int>> Transitions { get; } = [];
    public bool Accept { get; set; }

    public override string ToString() => $"q{Id}";
}
