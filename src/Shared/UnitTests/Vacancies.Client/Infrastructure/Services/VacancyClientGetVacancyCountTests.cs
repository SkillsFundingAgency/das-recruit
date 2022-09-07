using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class VacancyClientGetVacancyCountTests
    {
        [Test, MoqAutoData]
        public async Task Then_Returns_Vacancies_Count_For_Provider(
            long ukprn,
            long vacancyCount,
            string searchTerm,
            VacancyType vacancyType,
            FilteringOptions? filteringOptions,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            VacancyClient vacancyClient)
        {
            vacanciesSummaryProvider.Setup(x => x.VacancyCount(ukprn, string.Empty, vacancyType, filteringOptions, searchTerm, OwnerType.Provider)).ReturnsAsync(vacancyCount);

            var actual = await vacancyClient.GetVacancyCount(ukprn,  vacancyType, filteringOptions, searchTerm);

            actual.Should().Be(vacancyCount);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Returns_Vacancies_Count_For_Employer(
            long vacancyCount,
            string searchTerm,
            string employerAccountId,
            VacancyType vacancyType,
            FilteringOptions? filteringOptions,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            VacancyClient vacancyClient)
        {
            vacanciesSummaryProvider.Setup(x => x.VacancyCount(null, employerAccountId, vacancyType, filteringOptions, searchTerm, OwnerType.Employer)).ReturnsAsync(vacancyCount);

            var actual = await vacancyClient.GetVacancyCount(employerAccountId,  vacancyType, filteringOptions, searchTerm);

            actual.Should().Be(vacancyCount);
        }
    }
}