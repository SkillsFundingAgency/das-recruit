using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class VacancyClientGetVacancyCountTests
    {
        [Test, MoqAutoData]
        public async Task Then_Returns_Vacancies_Count_For_Provider(
            long ukprn,
            long vacancyCount,
            string searchTerm,
            FilteringOptions? filteringOptions,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            VacancyClient vacancyClient)
        {
            vacanciesSummaryProvider.Setup(x => x.VacancyCount(ukprn, string.Empty, filteringOptions, searchTerm, OwnerType.Provider)).ReturnsAsync(vacancyCount);

            var actual = await vacancyClient.GetVacancyCount(ukprn, filteringOptions, searchTerm);

            actual.Should().Be(vacancyCount);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Returns_Vacancies_Count_For_Employer(
            long vacancyCount,
            string searchTerm,
            string employerAccountId,
            FilteringOptions? filteringOptions,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            VacancyClient vacancyClient)
        {
            vacanciesSummaryProvider.Setup(x => x.VacancyCount(null, employerAccountId, filteringOptions, searchTerm, OwnerType.Employer)).ReturnsAsync(vacancyCount);

            var actual = await vacancyClient.GetVacancyCount(employerAccountId, filteringOptions, searchTerm);

            actual.Should().Be(vacancyCount);
        }
    }
}