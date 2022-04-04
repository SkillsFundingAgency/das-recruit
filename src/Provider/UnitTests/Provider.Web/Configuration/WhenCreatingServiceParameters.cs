using Esfa.Recruit.Provider.Web.Configuration;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Configuration
{
    public class WhenCreatingServiceParameters
    {
        [TestCase("apprenticeship", VacancyType.Apprenticeship)]
        [TestCase("traineeship", VacancyType.Traineeship)]
        [TestCase("degree", VacancyType.Apprenticeship)]
        public void Then_Correctly_Assigned_To_Recruit_Type(string recruitType, VacancyType expectedType)
        {
            var actual = new ServiceParameters(recruitType);

            actual.VacancyType.Should().Be(expectedType);
        }
    }
}