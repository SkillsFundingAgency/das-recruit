using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.VacancyReview.Requests;

public class WhenBuildingGetAnonymousApprovedCountByAccountLegalEntity
{
    [Test, AutoData]
    public void Then_The_Request_Is_Built_Correctly(long accountLegalEntityId)
    {
        var actual = new GetAnonymousApprovedCountByAccountLegalEntity(accountLegalEntityId);

        actual.GetUrl.Should().Be($"accounts/{accountLegalEntityId}/vacancyreviews");
    }
}