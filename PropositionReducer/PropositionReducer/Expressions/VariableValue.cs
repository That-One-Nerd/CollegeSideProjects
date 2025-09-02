namespace PropositionReducer.Expressions;

public class VariableValue(char signifier) : Expression
{
    public char Signifier { get; } = signifier;

    public override bool Evaluate(InputArray inputs) => inputs.Get(Signifier);
    public override string ToString() => Signifier.ToString();
}
