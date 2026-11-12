namespace TaskFlow.Domain.Functional;

public abstract record Option<T>
{
    public static Option<T> Some(T value) => new Some<T>(value);
    public static Option<T> None() => new None<T>();
    
    public abstract Option<TResult> Map<TResult>(Func<T, TResult> mapper);
    public abstract Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder);
    public abstract T GetValueOrDefault(T defaultValue);
    public abstract T GetValueOrDefault(Func<T> defaultFactory);
    public abstract bool IsSome { get; }
    public bool IsNone => !IsSome;
    public abstract void Match(Action<T> onSome, Action onNone);
    public abstract TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone);
}

public sealed record Some<T>(T Value) : Option<T>
{
    public override Option<TResult> Map<TResult>(Func<T, TResult> mapper) => Option<TResult>.Some(mapper(Value));
    public override Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder) => binder(Value);
    public override T GetValueOrDefault(T defaultValue) => Value;
    public override T GetValueOrDefault(Func<T> defaultFactory) => Value;
    public override bool IsSome => true;
    public override void Match(Action<T> onSome, Action onNone) => onSome(Value);
    public override TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone) => onSome(Value);
}

public sealed record None<T> : Option<T>
{
    public override Option<TResult> Map<TResult>(Func<T, TResult> mapper) => Option<TResult>.None();
    public override Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder) => Option<TResult>.None();
    public override T GetValueOrDefault(T defaultValue) => defaultValue;
    public override T GetValueOrDefault(Func<T> defaultFactory) => defaultFactory();
    public override bool IsSome => false;
    public override void Match(Action<T> onSome, Action onNone) => onNone();
    public override TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone) => onNone();
}

public static class OptionExtensions
{
    public static Option<T> ToOption<T>(this T? value) where T : class =>
        value is not null ? Option<T>.Some(value) : Option<T>.None();
    
    public static Option<T> ToOption<T>(this T? value) where T : struct =>
        value.HasValue ? Option<T>.Some(value.Value) : Option<T>.None();
    
    public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate) =>
        option.Bind(v => predicate(v) ? Option<T>.Some(v) : Option<T>.None());
}
