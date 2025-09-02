namespace PropositionReducer.Expressions;

public class NegationOperator(Expression inner) : UnaryOperator(inner)
{
    public override bool Evaluate(InputArray inputs) => !Inner.Evaluate(inputs);
    public override string ToString() => $@"\neg {Inner}";
}
