namespace TaskFlow.Web.Tests;

using TaskFlow.Web.Services;
using TaskFlow.Web.State;
using Xunit;

public class WebStateAndServicesTests
{
    [Fact]
    public void ToastService_ShowSuccess_RaisesOnShowEvent()
    {
        // Arrange
        var toastService = new ToastService();
        ToastMessage? captured = null;
        toastService.OnShow += msg => captured = msg;

        // Act
        toastService.ShowSuccess("Operation succeeded!", "Great");

        // Assert
        Assert.NotNull(captured);
        Assert.Equal("Operation succeeded!", captured.Message);
        Assert.Equal("Great", captured.Title);
        Assert.Equal(ToastLevel.Success, captured.Level);
    }

    [Fact]
    public void ToastService_ShowError_RaisesOnShowEvent()
    {
        // Arrange
        var toastService = new ToastService();
        ToastMessage? captured = null;
        toastService.OnShow += msg => captured = msg;

        // Act
        toastService.ShowError("Operation failed!", "Oops");

        // Assert
        Assert.NotNull(captured);
        Assert.Equal("Operation failed!", captured.Message);
        Assert.Equal("Oops", captured.Title);
        Assert.Equal(ToastLevel.Error, captured.Level);
    }

    [Fact]
    public void TaskStateContainer_NotifyStateChanged_TriggersOnChange()
    {
        // Arrange
        var state = new TaskStateContainer();
        var triggered = false;
        state.OnChange += () => triggered = true;

        // Act
        state.NotifyStateChanged();

        // Assert
        Assert.True(triggered);
    }
}
