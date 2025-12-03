namespace Tuat.FlowAutomation.StepTypes.Comparer;

public class CompareOperator(int Id, string DisplaySymbol)
{
    public int Id { get; set; } = Id;
    public string DisplaySymbol { get; set; } = DisplaySymbol;

    public static List<CompareOperator> Operators = new()
    {
        new CompareOperatorEqual(),
        new CompareOperatorNotEqual(),
        new CompareOperatorSmaller(),
        new CompareOperatorSmallerOrEqual(),
        new CompareOperatorGreater(),
        new CompareOperatorGreaterOrEqual(),
    };

    public virtual bool Evaluate(object? reference, object? value) => false;

}

public class CompareOperatorEqual() : CompareOperator(1, "== (equal)")
{
    public override bool Evaluate(object? reference, object? value) => object.Equals(reference, value);
}

public class CompareOperatorNotEqual() : CompareOperator(2, "<> (not equal)")
{
    public override bool Evaluate(object? reference, object? value) => !object.Equals(reference, value);
}

public class CompareOperatorSmaller() : CompareOperator(3, "< (smaller)")
{
    public override bool Evaluate(object? reference, object? value)
    {
        if (reference is IComparable refComp && value is IComparable valComp)
        {
            return refComp.CompareTo(valComp) < 0;
        }
        return false;
    }
}

public class CompareOperatorSmallerOrEqual() : CompareOperator(4, "<= (smaller or equal)")
{
    public override bool Evaluate(object? reference, object? value)
    {
        if (reference is IComparable refComp && value is IComparable valComp)
        {
            return refComp.CompareTo(valComp) <= 0;
        }
        return false;
    }
}

public class CompareOperatorGreater() : CompareOperator(5, "> (greater)")
{
    public override bool Evaluate(object? reference, object? value)
    {
        if (reference is IComparable refComp && value is IComparable valComp)
        {
            return refComp.CompareTo(valComp) > 0;
        }
        return false;
    }
}

public class CompareOperatorGreaterOrEqual() : CompareOperator(6, ">= (greator or equal)")
{
    public override bool Evaluate(object? reference, object? value)
    {
        if (reference is IComparable refComp && value is IComparable valComp)
        {
            return refComp.CompareTo(valComp) >= 0;
        }
        return false;
    }
}
