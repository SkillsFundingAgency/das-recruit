using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Configuration
{
    public class WhenCreatingServiceParameters
    {
        [TestCase("apprenticeship", VacancyType.Apprenticeship)]
        [TestCase("degree", VacancyType.Apprenticeship)]
        public void Then_Correctly_Assigned_To_Recruit_Type(string recruitType, VacancyType expectedType)
        {
            var actual = new ServiceParameters();

            actual.VacancyType.Should().Be(expectedType);
        }
    }
}