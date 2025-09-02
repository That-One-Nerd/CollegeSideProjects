namespace PropositionReducer.Expressions;

public abstract class UnaryOperator(Expression inner) : Expression
{
    public Expression Inner { get; } = inner;
}
