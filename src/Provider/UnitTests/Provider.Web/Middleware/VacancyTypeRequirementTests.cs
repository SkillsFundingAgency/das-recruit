using Esfa.Recruit.Provider.Web.Middleware;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Middleware
{
    public class VacancyTypeRequirementTests
    {
        [Theory, AutoFixture.Xunit2.AutoData]
        public void WhenConstructing_ThenPopulatesVacancyType(VacancyType vacancyType)
        {
            var requirement = new VacancyTypeRequirement(vacancyType);

            requirement.VacancyType.Should().Be(vacancyType);
        }
    }
}