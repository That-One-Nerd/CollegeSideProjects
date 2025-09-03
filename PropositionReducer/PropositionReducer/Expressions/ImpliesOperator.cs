namespace PropositionReducer.Expressions;

public class ImpliesOperator(Expression left, Expression right) : BinaryOperator(left, right)
{
    public override bool Evaluate(InputArray inputs)
    {
        if (Left.Evaluate(inputs)) return Right.Evaluate(inputs);
        else return true;
    }
    public override string ToString() => $@"{Left} ─→ {Right}";
}
