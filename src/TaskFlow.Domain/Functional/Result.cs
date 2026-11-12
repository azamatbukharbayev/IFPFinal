namespace TaskFlow.Domain.Functional;

public abstract record Result<T>
{
    public static Result<T> Success(T value) => new Success<T>(value);
    public static Result<T> Failure(string error) => new Failure<T>(error);
    public static Result<T> Failure(IEnumerable<string> errors) => new Failure<T>(errors);
    
    public abstract Result<TResult> Map<TResult>(Func<T, TResult> mapper);
    public abstract Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder);
    public abstract T GetValueOrDefault(T defaultValue);
    public abstract bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public abstract TResult Match<TResult>(Func<T, TResult> onSuccess, Func<IReadOnlyList<string>, TResult> onFailure);
    public abstract void Match(Action<T> onSuccess, Action<IReadOnlyList<string>> onFailure);
    public abstract IReadOnlyList<string> Errors { get; }
}

public sealed record Success<T>(T Value) : Result<T>
{
    public override Result<TResult> Map<TResult>(Func<T, TResult> mapper) => Result<TResult>.Success(mapper(Value));
    public override Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder) => binder(Value);
    public override T GetValueOrDefault(T defaultValue) => Value;
    public override bool IsSuccess => true;
    public override TResult Match<TResult>(Func<T, TResult> onSuccess, Func<IReadOnlyList<string>, TResult> onFailure) => onSuccess(Value);
    public override void Match(Action<T> onSuccess, Action<IReadOnlyList<string>> onFailure) => onSuccess(Value);
    public override IReadOnlyList<string> Errors => Array.Empty<string>();
}

public sealed record Failure<T> : Result<T>
{
    private readonly IReadOnlyList<string> _errors;
    
    public Failure(string error) => _errors = new[] { error };
    public Failure(IEnumerable<string> errors) => _errors = errors.ToList().AsReadOnly();
    
    public override Result<TResult> Map<TResult>(Func<T, TResult> mapper) => Result<TResult>.Failure(_errors);
    public override Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder) => Result<TResult>.Failure(_errors);
    public override T GetValueOrDefault(T defaultValue) => defaultValue;
    public override bool IsSuccess => false;
    public override TResult Match<TResult>(Func<T, TResult> onSuccess, Func<IReadOnlyList<string>, TResult> onFailure) => onFailure(_errors);
    public override void Match(Action<T> onSuccess, Action<IReadOnlyList<string>> onFailure) => onFailure(_errors);
    public override IReadOnlyList<string> Errors => _errors;
}

public static class ResultExtensions
{
    public static Result<T> ToResult<T>(this T? value, string errorMessage) where T : class =>
        value is not null ? Result<T>.Success(value) : Result<T>.Failure(errorMessage);
    
    public static Result<(T1, T2)> Combine<T1, T2>(this Result<T1> first, Result<T2> second) =>
        (first, second) switch
        {
            (Success<T1> s1, Success<T2> s2) => Result<(T1, T2)>.Success((s1.Value, s2.Value)),
            _ => Result<(T1, T2)>.Failure(first.Errors.Concat(second.Errors))
        };
}
