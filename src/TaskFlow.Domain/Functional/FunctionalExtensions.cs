namespace TaskFlow.Domain.Functional;

public static class FunctionalExtensions
{
    // Currying
    public static Func<T1, Func<T2, TResult>> Curry<T1, T2, TResult>(this Func<T1, T2, TResult> func) =>
        a => b => func(a, b);
    
    public static Func<T1, Func<T2, Func<T3, TResult>>> Curry<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func) =>
        a => b => c => func(a, b, c);
    
    // Partial Application
    public static Func<T2, TResult> Partial<T1, T2, TResult>(this Func<T1, T2, TResult> func, T1 arg1) =>
        arg2 => func(arg1, arg2);
    
    public static Func<T2, T3, TResult> Partial<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func, T1 arg1) =>
        (arg2, arg3) => func(arg1, arg2, arg3);
    
    // Function Composition: f.Then(g) == g(f(x))
    public static Func<T, TResult2> Then<T, TResult1, TResult2>(this Func<T, TResult1> first, Func<TResult1, TResult2> second) =>
        x => second(first(x));
    
    // Pipe: value.Pipe(f) == f(value)
    public static TResult Pipe<T, TResult>(this T value, Func<T, TResult> func) => func(value);
    
    // Tap: execute side-effect, return original value
    public static T Tap<T>(this T value, Action<T> action) { action(value); return value; }
    
    // Memoize
    public static Func<T, TResult> Memoize<T, TResult>(this Func<T, TResult> func) where T : notnull
    {
        var cache = new Dictionary<T, TResult>();
        return arg =>
        {
            if (!cache.TryGetValue(arg, out var result))
            {
                result = func(arg);
                cache[arg] = result;
            }
            return result;
        };
    }
    
    // Trampolining for stack-safe recursion
    public static T Trampoline<T>(Func<Bounce<T>> thunk)
    {
        var result = thunk();
        while (result is More<T> more)
            result = more.Next();
        return ((Done<T>)result).Value;
    }
}

public abstract record Bounce<T>;
public sealed record Done<T>(T Value) : Bounce<T>;
public sealed record More<T>(Func<Bounce<T>> Next) : Bounce<T>;
