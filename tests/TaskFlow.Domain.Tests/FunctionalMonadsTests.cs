namespace TaskFlow.Domain.Tests;

using Xunit;
using TaskFlow.Domain.Functional;

public class FunctionalMonadsTests
{
    [Fact]
    public void Option_Some_ReturnsValue()
    {
        var opt = Option<string>.Some("hello");
        Assert.True(opt.IsSome);
        Assert.False(opt.IsNone);
        Assert.Equal("hello", opt.GetValueOrDefault("default"));
    }

    [Fact]
    public void Option_None_ReturnsDefault()
    {
        var opt = Option<string>.None();
        Assert.False(opt.IsSome);
        Assert.True(opt.IsNone);
        Assert.Equal("fallback", opt.GetValueOrDefault("fallback"));
    }

    [Fact]
    public void Option_Map_TransformsValue()
    {
        var opt = Option<int>.Some(42);
        var mapped = opt.Map(x => x * 2);
        Assert.Equal(84, mapped.GetValueOrDefault(0));

        var none = Option<int>.None();
        var mappedNone = none.Map(x => x * 2);
        Assert.True(mappedNone.IsNone);
    }

    [Fact]
    public void Option_Bind_ChainsOperations()
    {
        var opt = Option<int>.Some(10);
        var bound = opt.Bind(x => x > 5 ? Option<string>.Some($"Value: {x}") : Option<string>.None());
        Assert.Equal("Value: 10", bound.GetValueOrDefault(""));

        var small = Option<int>.Some(2);
        var boundSmall = small.Bind(x => x > 5 ? Option<string>.Some($"Value: {x}") : Option<string>.None());
        Assert.True(boundSmall.IsNone);
    }

    [Fact]
    public void Option_Match_ExecutesCorrectBranch()
    {
        var opt = Option<int>.Some(7);
        var result = opt.Match(v => v * 3, () => 0);
        Assert.Equal(21, result);

        var none = Option<int>.None();
        var noneResult = none.Match(v => v * 3, () => -1);
        Assert.Equal(-1, noneResult);
    }

    [Fact]
    public void Option_Where_FiltersValue()
    {
        var opt = Option<int>.Some(10);
        var kept = opt.Where(x => x > 5);
        var discarded = opt.Where(x => x > 15);
        Assert.True(kept.IsSome);
        Assert.True(discarded.IsNone);
    }

    [Fact]
    public void Result_Success_ContainsValue()
    {
        var res = Result<int>.Success(100);
        Assert.True(res.IsSuccess);
        Assert.False(res.IsFailure);
        Assert.Equal(100, res.GetValueOrDefault(0));
        Assert.Empty(res.Errors);
    }

    [Fact]
    public void Result_Failure_ContainsErrors()
    {
        var res = Result<int>.Failure("Invalid input");
        Assert.False(res.IsSuccess);
        Assert.True(res.IsFailure);
        Assert.Equal(0, res.GetValueOrDefault(0));
        Assert.Single(res.Errors);
        Assert.Equal("Invalid input", res.Errors[0]);
    }

    [Fact]
    public void Result_Map_TransformsSuccess()
    {
        var res = Result<int>.Success(10);
        var mapped = res.Map(x => x + 5);
        Assert.Equal(15, mapped.GetValueOrDefault(0));

        var fail = Result<int>.Failure("Error");
        var mappedFail = fail.Map(x => x + 5);
        Assert.True(mappedFail.IsFailure);
    }

    [Fact]
    public void Result_Bind_ChainsPotentiallyFailingOperations()
    {
        Func<int, Result<int>> divideTenBy = x =>
            x == 0 ? Result<int>.Failure("Division by zero") : Result<int>.Success(10 / x);

        var ok = Result<int>.Success(2).Bind(divideTenBy);
        Assert.True(ok.IsSuccess);
        Assert.Equal(5, ok.GetValueOrDefault(0));

        var divZero = Result<int>.Success(0).Bind(divideTenBy);
        Assert.True(divZero.IsFailure);
        Assert.Contains("Division by zero", divZero.Errors);
    }

    [Fact]
    public void Result_Combine_CombinesSuccessesAndAggregatesFailures()
    {
        var r1 = Result<string>.Success("A");
        var r2 = Result<int>.Success(1);
        var combined = r1.Combine(r2);
        Assert.True(combined.IsSuccess);
        Assert.Equal(("A", 1), combined.GetValueOrDefault(("", 0)));

        var f1 = Result<string>.Failure("Error 1");
        var f2 = Result<int>.Failure("Error 2");
        var combinedFail = f1.Combine(f2);
        Assert.True(combinedFail.IsFailure);
        Assert.Equal(2, combinedFail.Errors.Count);
    }
}
