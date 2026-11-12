namespace TaskFlow.Domain.Tests;

using Xunit;
using TaskFlow.Domain.Functional;

public class FunctionalExtensionsTests
{
    [Fact]
    public void Curry_TransformsMultiArgFunction()
    {
        Func<int, int, int> add = (a, b) => a + b;
        var curriedAdd = add.Curry();
        var addFive = curriedAdd(5);
        Assert.Equal(12, addFive(7));
    }

    [Fact]
    public void Partial_AppliesFirstArgument()
    {
        Func<string, string, string> greet = (salutation, name) => $"{salutation}, {name}!";
        var sayHello = greet.Partial("Hello");
        Assert.Equal("Hello, Alice!", sayHello("Alice"));
        Assert.Equal("Hello, Bob!", sayHello("Bob"));
    }

    [Fact]
    public void FunctionComposition_Then_CombinesFunctions()
    {
        Func<int, int> doubleIt = x => x * 2;
        Func<int, string> format = x => $"Result: {x}";

        var pipeline = doubleIt.Then(format);
        Assert.Equal("Result: 20", pipeline(10));
    }

    [Fact]
    public void Pipe_PassesValueThroughFunction()
    {
        var result = 5
            .Pipe(x => x * 3)
            .Pipe(x => x + 2)
            .Pipe(x => x.ToString());

        Assert.Equal("17", result);
    }

    [Fact]
    public void Memoize_CachesPureFunctionResults()
    {
        var callCount = 0;
        Func<int, int> expensive = x =>
        {
            callCount++;
            return x * x;
        };

        var memoized = expensive.Memoize();

        Assert.Equal(16, memoized(4));
        Assert.Equal(1, callCount);

        Assert.Equal(16, memoized(4)); // Cached!
        Assert.Equal(1, callCount);

        Assert.Equal(25, memoized(5));
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Trampoline_ExecutesDeepRecursionWithoutStackOverflow()
    {
        // Calculate sum from 1 to N with trampoline
        Bounce<long> SumBounce(long n, long acc)
        {
            if (n <= 0) return new Done<long>(acc);
            return new More<long>(() => SumBounce(n - 1, acc + n));
        }

        var result = FunctionalExtensions.Trampoline(() => SumBounce(10000, 0));
        Assert.Equal(50005000L, result);
    }
}
