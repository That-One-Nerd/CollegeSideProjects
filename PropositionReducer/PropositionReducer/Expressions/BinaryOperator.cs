namespace PropositionReducer.Expressions;

public abstract class BinaryOperator(Expression left, Expression right) : Expression
{
    public Expression Left { get; } = left;
    public Expression Right { get; } = right;
}
