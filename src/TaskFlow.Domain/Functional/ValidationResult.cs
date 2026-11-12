namespace TaskFlow.Domain.Functional;

public abstract record ValidationResult
{
    public static ValidationResult Valid() => new ValidationSuccess();
    public static ValidationResult Invalid(params string[] errors) => new ValidationFailure(errors.ToList());
    
    public abstract bool IsValid { get; }
    public abstract IReadOnlyList<string> Errors { get; }
    
    public static ValidationResult Combine(params ValidationResult[] results)
    {
        var errors = results.SelectMany(r => r.Errors).ToList();
        return errors.Count == 0 ? Valid() : Invalid(errors.ToArray());
    }
}

public sealed record ValidationSuccess : ValidationResult
{
    public override bool IsValid => true;
    public override IReadOnlyList<string> Errors => Array.Empty<string>();
}

public sealed record ValidationFailure(IReadOnlyList<string> ValidationErrors) : ValidationResult
{
    public override bool IsValid => false;
    public override IReadOnlyList<string> Errors => ValidationErrors;
}
