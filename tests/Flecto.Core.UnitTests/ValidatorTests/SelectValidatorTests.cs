using Flecto.Core.Validators;

namespace Flecto.Core.UnitTests.ValidatorTests;

public class SelectValidatorTests
{
    [Fact]
    public void EnsureValid_SelectAlreadyCalled_ThrowsInvalidOperationException()
    {
        // Arrange
        var selectWasSet = true;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() =>
            SelectValidator.EnsureValid(selectWasSet));

        // Assert
        Assert.Equal("Select can only be called once per query", ex.Message);
    }

    [Fact]
    public void EnsureValid_ValidTablesAndSelectNotSet_DoesNotThrow()
    {
        // Arrange
        var selectWasSet = false;
        var tablesWithColumns = new[] { ("users", new[] { "id", "name" }) };

        // Act
        var ex = Record.Exception(() =>
            SelectValidator.EnsureValid(selectWasSet, tablesWithColumns));

        // Assert
        Assert.Null(ex);
    }
}
