using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Mappings.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Mappings
{
    public class ApplicationReviewMapperExtensionsTest
    {
        [Test, AutoData]
        public void When_Mapping_To_View_Model(
            ApplicationReview applicationReview,
            Vacancy vacancy)
        {
            var source = applicationReview;
            var actual = source.ToViewModel();

            actual.Email.Should().Be(source.Application.Email);
            actual.AdditionalQuestion1.Should().Be(source.AdditionalQuestion1);
            actual.AdditionalQuestion2.Should().Be(source.AdditionalQuestion2);
            actual.AdditionalAnswer1.Should().Be(source.Application.AdditionalQuestion1);
            actual.AdditionalAnswer2.Should().Be(source.Application.AdditionalQuestion2);
        }
            
    }
}
