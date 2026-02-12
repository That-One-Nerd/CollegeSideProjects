namespace DfaSim;

public class State(int id)
{
    public int Id { get; } = id;
    public Dictionary<char, int> Transitions { get; } = [];
    public bool Accept { get; set; }
}
