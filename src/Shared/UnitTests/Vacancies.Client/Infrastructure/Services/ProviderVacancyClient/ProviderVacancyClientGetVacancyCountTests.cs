using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services.ProviderVacancyClient
{
    public class ProviderVacancyClientGetVacancyCountTests
    {
        [Test, MoqAutoData]
        public async Task Then_Returns_Vacancies_Count(
            long ukprn,
            long vacancyCount,
            VacancyType vacancyType,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            VacancyClient vacancyClient)
        {
            vacanciesSummaryProvider.Setup(x => x.VacancyCount(ukprn, vacancyType)).ReturnsAsync(vacancyCount);

            var actual = await vacancyClient.GetVacancyCount(ukprn, vacancyType);

            actual.Should().Be(vacancyCount);
        }
    }
}