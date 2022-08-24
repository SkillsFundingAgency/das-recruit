using AutoFixture.Xunit2;
using Esfa.Recruit.Provider.Web.Middleware;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Middleware
{
    public class VacancyTypeRequirementTests
    {
        [Theory, AutoData]
        public void WhenConstructing_ThenPopulatesVacancyType(VacancyType vacancyType)
        {
            var requirement = new VacancyTypeRequirement(vacancyType);

            requirement.VacancyType.Should().Be(vacancyType);
        }
    }
}