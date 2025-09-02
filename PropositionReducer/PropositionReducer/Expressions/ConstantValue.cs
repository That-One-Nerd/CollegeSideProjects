namespace PropositionReducer.Expressions;

public class ConstantValue(bool value) : Expression
{
    public bool Value { get; } = value;

    public override bool Evaluate(InputArray inputs) => Value;
    public override string ToString() => Value ? "T" : "F";
}
