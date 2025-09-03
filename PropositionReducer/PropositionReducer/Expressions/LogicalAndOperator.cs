namespace PropositionReducer.Expressions;

public class LogicalAndOperator(Expression left, Expression right) : BinaryOperator(left, right)
{
    public override bool Evaluate(InputArray inputs) => Left.Evaluate(inputs) && Right.Evaluate(inputs);
    public override string ToString() => $@"{Left} ∧ {Right}";
}
