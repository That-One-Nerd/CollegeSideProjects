namespace PropositionReducer.Expressions;

public class ParenthesisExpression(Expression inner) : Expression
{
    public Expression Inner { get; } = inner;

    public override bool Evaluate(InputArray inputs) => Inner.Evaluate(inputs);
    public override string ToString() => $"({Inner})";
}
