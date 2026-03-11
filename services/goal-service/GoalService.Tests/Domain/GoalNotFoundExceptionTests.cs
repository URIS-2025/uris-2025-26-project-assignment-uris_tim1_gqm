using FluentAssertions;
using GoalService.Domain.Exceptions;
using Xunit;

namespace GoalService.Tests.Domain;

public class GoalNotFoundExceptionTests
{
    [Fact]
    public void Exception_ShouldContainGoalId_InMessage()
    {
        // Arrange
        var goalId = Guid.NewGuid();

        // Act
        var exception = new GoalNotFoundException(goalId);

        // Assert
        exception.Message.Should().Contain(goalId.ToString());
        exception.Message.Should().Contain("was not found");
    }
}
