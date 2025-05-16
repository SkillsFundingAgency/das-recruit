using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests
{
    [TestFixture]
    public class WhenBuildingGetEmployerApplicationReviewsCountApiRequest
    {
        [Test, AutoData]
        public void Then_It_Is_Correctly_Constructed(
            long accountId,
            List<long> vacancyReferences)
        {
            var actual = new GetEmployerApplicationReviewsCountApiRequest(accountId, vacancyReferences);

            actual.PostUrl.Should().Be($"employerAccounts/{accountId}/count");
            ((List<long>)actual.Data).Should().BeEquivalentTo(vacancyReferences);
        }
    }
}
