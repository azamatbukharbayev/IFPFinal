namespace TaskFlow.Domain.ValueObjects;
public sealed record Effort(decimal Hours)
{
    public static Effort Zero => new(0m);
    public static Effort Create(decimal hours) => hours < 0 
        ? throw new ArgumentException("Effort cannot be negative.", nameof(hours)) 
        : new Effort(hours);
    public static Effort operator +(Effort a, Effort b) => new(a.Hours + b.Hours);
}
