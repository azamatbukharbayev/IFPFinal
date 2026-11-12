namespace TaskFlow.Domain.ValueObjects;
public sealed record PriorityScore(double Value, string Explanation)
{
    public static PriorityScore Create(double value, string explanation) =>
        new(Math.Clamp(value, 0.0, 100.0), explanation);
}
