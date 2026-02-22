using FluentAssertions;
using GoalService.Domain.Exceptions;
using System;
using Xunit;

namespace GoalService.Tests.Domain;

public class GoalHierarchyCycleExceptionTests
{
    [Fact]
    public void Exception_ShouldContainTargetGoalAndStrategyIds_InMessage()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();

        // Act
        var exception = new GoalHierarchyCycleException(goalId, strategyId);

        // Assert
        exception.Message.Should().Contain(goalId.ToString());
        exception.Message.Should().Contain(strategyId.ToString());
        exception.Message.Should().Contain("would create a cycle");
    }
}
