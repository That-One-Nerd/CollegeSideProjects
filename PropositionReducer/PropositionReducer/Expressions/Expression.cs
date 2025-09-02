namespace PropositionReducer.Expressions;

public abstract class Expression
{
    public abstract bool Evaluate(InputArray inputs);
    public abstract override string ToString();
}
