using Domain.Specifications;
using FluentAssertions;

namespace Tests.Unit.Domain.Specifications;

public class SingleChargeSpecificationTests
{
    [Fact]
    public void GroupByChargeWindow_EmptyList_ReturnsNoGroups()
    {
        // Arrange
        var timestamps = Array.Empty<DateTime>();

        // Act
        var result = SingleChargeSpecification.GroupByChargeWindow(timestamps, 60).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GroupByChargeWindow_SingleTimestamp_ReturnsOneGroup()
    {
        // Arrange
        var timestamps = new[] { new DateTime(2013, 2, 7, 6, 0, 0) };

        // Act
        var result = SingleChargeSpecification.GroupByChargeWindow(timestamps, 60).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(1);
        result[0][0].Should().Be(timestamps[0]);
    }

    [Fact]
    public void GroupByChargeWindow_TwoTimestampsWithinWindow_ReturnsOneGroup()
    {
        // Arrange
        var timestamps = new[]
        {
            new DateTime(2013, 2, 8, 6, 20, 27),
            new DateTime(2013, 2, 8, 6, 27, 0),
        };

        // Act
        var result = SingleChargeSpecification.GroupByChargeWindow(timestamps, 60).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(2);
    }

    [Fact]
    public void GroupByChargeWindow_TwoTimestampsExactly60MinutesApart_ReturnsOneGroup()
    {
        // Arrange - <= 60 minutes means they're in same window
        var timestamps = new[]
        {
            new DateTime(2013, 2, 7, 6, 0, 0),
            new DateTime(2013, 2, 7, 7, 0, 0),
        };

        // Act
        var result = SingleChargeSpecification.GroupByChargeWindow(timestamps, 60).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(2);
    }

    [Fact]
    public void GroupByChargeWindow_TwoTimestampsOver60MinutesApart_ReturnsTwoGroups()
    {
        // Arrange
        var timestamps = new[]
        {
            new DateTime(2013, 2, 7, 6, 0, 0),
            new DateTime(2013, 2, 7, 7, 5, 0),
        };

        // Act
        var result = SingleChargeSpecification.GroupByChargeWindow(timestamps, 60).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void GroupByChargeWindow_ThreeTimestampsWithinWindow_ReturnsOneGroup()
    {
        // Arrange
        var timestamps = new[]
        {
            new DateTime(2013, 2, 8, 15, 29, 0),
            new DateTime(2013, 2, 8, 15, 47, 0),
            new DateTime(2013, 2, 8, 16, 1, 0),
        };

        // Act
        var result = SingleChargeSpecification.GroupByChargeWindow(timestamps, 60).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(3);
    }

    [Fact]
    public void GroupByChargeWindow_MultipleGroups_ReturnsCorrectGrouping()
    {
        // Arrange
        var timestamps = new[]
        {
            new DateTime(2013, 2, 8, 6, 20, 27),
            new DateTime(2013, 2, 8, 6, 27, 0),
            new DateTime(2013, 2, 8, 14, 35, 0),
            new DateTime(2013, 2, 8, 15, 29, 0),
            new DateTime(2013, 2, 8, 15, 47, 0),
            new DateTime(2013, 2, 8, 16, 1, 0),
        };

        // Act
        var result = SingleChargeSpecification.GroupByChargeWindow(timestamps, 60).ToList();

        // Assert
        // 14:35 to 15:29 is 54 minutes (same window)
        // 15:47 is 72 minutes from 14:35 (new window)
        // 15:47 to 16:01 is 14 minutes (same window)
        result.Should().HaveCount(3);
        result[0].Should().HaveCount(2); // 6:20 and 6:27
        result[1].Should().HaveCount(2); // 14:35, 15:29
        result[2].Should().HaveCount(2); // 15:47, 16:01
    }

    [Fact]
    public void GroupByChargeWindow_UnsortedTimestamps_SortsAndGroupsCorrectly()
    {
        // Arrange
        var timestamps = new[]
        {
            new DateTime(2013, 2, 8, 15, 29, 0),
            new DateTime(2013, 2, 8, 6, 27, 0),
            new DateTime(2013, 2, 8, 6, 20, 27),
        };

        // Act
        var result = SingleChargeSpecification.GroupByChargeWindow(timestamps, 60).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().HaveCount(2); // 6:20 and 6:27
        result[1].Should().HaveCount(1); // 15:29
    }

    [Fact]
    public void GroupByChargeWindow_ComplexScenario_GroupsCorrectly()
    {
        // Arrange - Friday 2013-02-08 from assignment
        var timestamps = new[]
        {
            new DateTime(2013, 2, 8, 6, 20, 27),
            new DateTime(2013, 2, 8, 6, 27, 0),
            new DateTime(2013, 2, 8, 14, 35, 0),
            new DateTime(2013, 2, 8, 15, 29, 0),
            new DateTime(2013, 2, 8, 15, 47, 0),
            new DateTime(2013, 2, 8, 16, 1, 0),
            new DateTime(2013, 2, 8, 16, 48, 0),
            new DateTime(2013, 2, 8, 17, 49, 0),
            new DateTime(2013, 2, 8, 18, 29, 0),
            new DateTime(2013, 2, 8, 18, 35, 0),
        };

        // Act
        var result = SingleChargeSpecification.GroupByChargeWindow(timestamps, 60).ToList();

        // Assert
        result.Should().HaveCount(5);
        result[0].Should().HaveCount(2); // Window 1: 06:20, 06:27
        result[1].Should().HaveCount(2); // Window 2: 14:35, 15:29 (54 min apart)
        result[2].Should().HaveCount(2); // Window 3: 15:47, 16:01 (14 min apart)
        result[3].Should().HaveCount(1); // Window 4: 16:48
        result[4].Should().HaveCount(3); // Window 5: 17:49, 18:29, 18:35 (all within 60 min of 17:49)
    }

    [Fact]
    public void GroupByChargeWindow_CustomWindowSize_GroupsCorrectly()
    {
        // Arrange
        var timestamps = new[]
        {
            new DateTime(2013, 2, 7, 6, 0, 0),
            new DateTime(2013, 2, 7, 6, 20, 0),
            new DateTime(2013, 2, 7, 6, 40, 0),
        };

        // Act - 30 minute window
        var result = SingleChargeSpecification.GroupByChargeWindow(timestamps, 30).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().HaveCount(2); // 6:00 and 6:20
        result[1].Should().HaveCount(1); // 6:40
    }

    [Fact]
    public void GroupByChargeWindow_TimestampsAtWindowBoundary_GroupsCorrectly()
    {
        // Arrange - exactly at 60 minute boundary
        var timestamps = new[]
        {
            new DateTime(2013, 2, 7, 6, 0, 0),
            new DateTime(2013, 2, 7, 6, 59, 59),
            new DateTime(2013, 2, 7, 7, 0, 0),
        };

        // Act
        var result = SingleChargeSpecification.GroupByChargeWindow(timestamps, 60).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(3); // All within 60 minutes
    }
}
