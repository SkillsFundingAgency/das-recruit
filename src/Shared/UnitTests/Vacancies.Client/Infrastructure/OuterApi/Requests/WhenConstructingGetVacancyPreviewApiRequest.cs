using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class WhenConstructingGetVacancyPreviewApiRequest
{
    [Test, AutoData]
    public void Then_The_Request_Is_Built(int standardId)
    {
        var actual = new GetVacancyPreviewApiRequest(standardId);

        actual.GetUrl.Should().Be($"vacancypreview?standardId={standardId}");
    }
}