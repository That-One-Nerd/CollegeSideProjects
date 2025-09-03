namespace PropositionReducer.Expressions;

public class LogicalOrOperator(Expression left, Expression right) : BinaryOperator(left, right)
{
    public override bool Evaluate(InputArray inputs) => Left.Evaluate(inputs) || Right.Evaluate(inputs);
    public override string ToString() => $@"{Left} ∨ {Right}";
}
