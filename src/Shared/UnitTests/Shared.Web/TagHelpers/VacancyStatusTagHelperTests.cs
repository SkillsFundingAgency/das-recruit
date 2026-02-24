using Esfa.Recruit.Shared.Web.TagHelpers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public class VacancyStatusTagHelperTests : TagHelperTestsBase
{
    [TestCase(VacancyStatus.Draft, "<strong class=\"govuk-tag govuk-tag--grey\">Draft</strong>")]
    [TestCase(VacancyStatus.Submitted, "<strong class=\"govuk-tag govuk-tag--blue\">Pending DfE review</strong>")]
    [TestCase(VacancyStatus.Live, "<strong class=\"govuk-tag govuk-tag--turquoise\">Live</strong>")]
    public async Task VacancyStatus_Renders_Canonical_Class(VacancyStatus status, string expected)
    {
        // Arrange
        var sut = new VacancyStatusTagHelper { Status = status };

        // Act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // Assert
        TagHelperOutput.AsString().Should().Be(expected);
    }
}