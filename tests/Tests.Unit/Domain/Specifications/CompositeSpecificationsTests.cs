using Domain.Specifications;
using FluentAssertions;

namespace Tests.Unit.Domain.Specifications;

public class CompositeSpecificationsTests
{
    // Simple test specifications
    private class AlwaysTrueSpecification : ISpecification<int>
    {
        public bool IsSatisfiedBy(int candidate) => true;
    }

    private class AlwaysFalseSpecification : ISpecification<int>
    {
        public bool IsSatisfiedBy(int candidate) => false;
    }

    private class IsEvenSpecification : ISpecification<int>
    {
        public bool IsSatisfiedBy(int candidate) => candidate % 2 == 0;
    }

    private class IsPositiveSpecification : ISpecification<int>
    {
        public bool IsSatisfiedBy(int candidate) => candidate > 0;
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    public void AndSpecification_CombinesTwoSpecs_WithLogicalAnd(
        bool left,
        bool right,
        bool expected
    )
    {
        // Arrange
        ISpecification<int> leftSpec = left
            ? new AlwaysTrueSpecification()
            : new AlwaysFalseSpecification();
        ISpecification<int> rightSpec = right
            ? new AlwaysTrueSpecification()
            : new AlwaysFalseSpecification();
        var andSpec = new AndSpecification<int>(leftSpec, rightSpec);

        // Act
        var result = andSpec.IsSatisfiedBy(42);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    public void OrSpecification_CombinesTwoSpecs_WithLogicalOr(bool left, bool right, bool expected)
    {
        // Arrange
        ISpecification<int> leftSpec = left
            ? new AlwaysTrueSpecification()
            : new AlwaysFalseSpecification();
        ISpecification<int> rightSpec = right
            ? new AlwaysTrueSpecification()
            : new AlwaysFalseSpecification();
        var orSpec = new OrSpecification<int>(leftSpec, rightSpec);

        // Act
        var result = orSpec.IsSatisfiedBy(42);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void NotSpecification_NegatesSpec_WithLogicalNot(bool input, bool expected)
    {
        // Arrange
        ISpecification<int> spec = input
            ? new AlwaysTrueSpecification()
            : new AlwaysFalseSpecification();
        var notSpec = new NotSpecification<int>(spec);

        // Act
        var result = notSpec.IsSatisfiedBy(42);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void AndExtension_CombinesSpecs_Fluently()
    {
        // Arrange
        var isEven = new IsEvenSpecification();
        var isPositive = new IsPositiveSpecification();
        var isEvenAndPositive = isEven.And(isPositive);

        // Act & Assert
        isEvenAndPositive.IsSatisfiedBy(4).Should().BeTrue(); // even and positive
        isEvenAndPositive.IsSatisfiedBy(-4).Should().BeFalse(); // even but negative
        isEvenAndPositive.IsSatisfiedBy(3).Should().BeFalse(); // positive but odd
        isEvenAndPositive.IsSatisfiedBy(-3).Should().BeFalse(); // negative and odd
    }

    [Fact]
    public void OrExtension_CombinesSpecs_Fluently()
    {
        // Arrange
        var isEven = new IsEvenSpecification();
        var isPositive = new IsPositiveSpecification();
        var isEvenOrPositive = isEven.Or(isPositive);

        // Act & Assert
        isEvenOrPositive.IsSatisfiedBy(4).Should().BeTrue(); // even and positive
        isEvenOrPositive.IsSatisfiedBy(-4).Should().BeTrue(); // even but negative
        isEvenOrPositive.IsSatisfiedBy(3).Should().BeTrue(); // positive but odd
        isEvenOrPositive.IsSatisfiedBy(-3).Should().BeFalse(); // negative and odd
    }

    [Fact]
    public void NotExtension_NegatesSpec_Fluently()
    {
        // Arrange
        var isEven = new IsEvenSpecification();
        var isOdd = isEven.Not();

        // Act & Assert
        isOdd.IsSatisfiedBy(3).Should().BeTrue();
        isOdd.IsSatisfiedBy(4).Should().BeFalse();
    }

    [Fact]
    public void ComplexComposition_CombinesMultipleSpecs_Correctly()
    {
        // Arrange - (IsEven AND IsPositive) OR (NOT IsEven AND NOT IsPositive)
        var isEven = new IsEvenSpecification();
        var isPositive = new IsPositiveSpecification();

        var evenAndPositive = isEven.And(isPositive);
        var oddAndNegative = isEven.Not().And(isPositive.Not());
        var complexSpec = evenAndPositive.Or(oddAndNegative);

        // Act & Assert
        complexSpec.IsSatisfiedBy(4).Should().BeTrue(); // even and positive
        complexSpec.IsSatisfiedBy(-3).Should().BeTrue(); // odd and negative
        complexSpec.IsSatisfiedBy(3).Should().BeFalse(); // odd and positive
        complexSpec.IsSatisfiedBy(-4).Should().BeFalse(); // even and negative
    }

    [Fact]
    public void RealWorldExample_SingleChargeWindowWithSameDayConstraint()
    {
        // Arrange - timestamps must be within 60 minutes AND on the same day
        var withinWindow = new SingleChargeWindowSpecification(60);
        var sameDay = new SameDaySpecification();
        var chargeWindowSpec = withinWindow.And(sameDay);

        var baseTime = new DateTime(2013, 2, 7, 23, 30, 0);
        var withinWindowSameDay = new DateTime(2013, 2, 7, 23, 45, 0);
        var withinWindowNextDay = new DateTime(2013, 2, 8, 0, 15, 0);

        // Act & Assert
        chargeWindowSpec.IsSatisfiedBy((baseTime, withinWindowSameDay)).Should().BeTrue();
        chargeWindowSpec.IsSatisfiedBy((baseTime, withinWindowNextDay)).Should().BeFalse();
    }

    // Helper specification for the real-world example
    private class SameDaySpecification : ISpecification<(DateTime first, DateTime second)>
    {
        public bool IsSatisfiedBy((DateTime first, DateTime second) candidate)
        {
            return candidate.first.Date == candidate.second.Date;
        }
    }
}
