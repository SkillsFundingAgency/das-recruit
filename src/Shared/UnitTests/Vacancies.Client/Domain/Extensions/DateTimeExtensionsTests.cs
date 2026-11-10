using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Domain.Extensions;

public class DateTimeExtensionsTests
{
    [Test]
    public void AsGdsDate_ShouldReturnFormattedDateString()
    {
        // Arrange
        var date = new DateTime(2023, 10, 5);

        // Act
        string result = date.AsGdsDate();

        // Assert
        result.Should().Be("5 Oct 2023");
    }

    [Test]
    public void AsGdsDate_Nullable_ShouldReturnFormattedDateString()
    {
        // Arrange
        DateTime? date = new DateTime(2023, 10, 5);

        // Act
        string result = date.AsGdsDate();

        // Assert
        result.Should().Be("5 Oct 2023");
    }

    [Test]
    public void AsGdsDate_WithLeading_Zero_ShouldReturnFormattedDateString()
    {
        // Arrange
        DateTime? date = new DateTime(2023, 10, 05);

        // Act
        string result = date.AsGdsDate();

        // Assert
        result.Should().Be("5 Oct 2023");
    }

    [Test]
    public void AsGdsDate_Nullable_ShouldReturnNull()
    {
        // Act
        string result = ((DateTime?) null).AsGdsDate();

        // Assert
        result.Should().BeNull();
    }
}