using Ark.Alliance.Core;
using Xunit;

/// <summary>
/// Exercises the <see cref="Unit"/> type equality and comparison operations.
/// </summary>
public class UnitTests
{
    /// <summary>
    /// Confirms <see cref="Unit.Value"/> equals the completed task result.
    /// </summary>
    [Fact]
    public async Task Should_be_equal_to_each_other()
    {
        var unit1 = Unit.Value;
        var unit2 = await Unit.Task;

        Assert.Equal(unit1, unit2);
        Assert.True(unit1 == unit2);
        Assert.False(unit1 != unit2);
    }

    /// <summary>
    /// Demonstrates <see cref="Unit"/> can be used as a dictionary key.
    /// </summary>
    [Fact]
    public void Should_be_equitable()
    {
        var dictionary = new Dictionary<Unit, string>
        {
            { new Unit(), "value" },
        };

        Assert.Equal("value", dictionary[default]);
    }

    /// <summary>
    /// Ensures <see cref="Unit.ToString"/> returns the canonical representation.
    /// </summary>
    [Fact]
    public void Should_tostring()
    {
        var unit = Unit.Value;
        Assert.Equal("()", unit.ToString());
    }

    /// <summary>
    /// Compares two units and expects zero as they are equal.
    /// </summary>
    [Fact]
    public void Should_compareto_as_zero()
    {
        var unit1 = new Unit();
        var unit2 = new Unit();

        Assert.Equal(0, unit1.CompareTo(unit2));
    }

    public static object[][] ValueData() => new[]
    {
        new object[] { new(), false },
        new object[] { "", false },
        new object[] { "()", false },
        new object[] { null!, false },
        new object[] { new Uri("https://www.google.com"), false },
        new object[] { new Unit(), true },
        new object[] { Unit.Value, true },
        new object[] { Unit.Task.Result, true },
        new object[] { default(Unit), true },
    };

    public static object[][] CompareToValueData() =>
        ValueData().Select(objects => new[] { objects[0] }).ToArray();

    /// <summary>
    /// Tests <see cref="Unit.Equals(object?)"/> against a variety of inputs.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValueData))]
    public void Should_be_equal(object value, bool isEqual)
    {
        var unit1 = Unit.Value;

        if (isEqual)
            Assert.True(unit1.Equals(value));
        else
            Assert.False(unit1.Equals(value));
    }

    /// <summary>
    /// Confirms <see cref="IComparable.CompareTo(object?)"/> returns zero for any unit value.
    /// </summary>
    [Theory]
    [MemberData(nameof(CompareToValueData))]
    public void Should_compareto_value_as_zero(object value)
    {
        var unit1 = new Unit();

        Assert.Equal(0, ((IComparable)unit1).CompareTo(value));
    }
}
