using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Mappings.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Mappings
{
    public class ApplicationReviewMapperExtensionsTest
    {
        [Test, AutoData]
        public async Task When_Mapping_To_View_Model(
            ApplicationReview applicationReview,
            Vacancy vacancy)
        {
            var source = applicationReview;
            var actual = source.ToViewModel(vacancy);

            actual.Email.Should().Be(source.Application.Email);
            actual.AdditionalQuestion1.Should().Be(vacancy.AdditionalQuestion1);
            actual.AdditionalQuestion2.Should().Be(vacancy.AdditionalQuestion2);
            actual.AdditionalAnswer1.Should().Be(source.Application.AdditionalQuestion1);
            actual.AdditionalAnswer2.Should().Be(source.Application.AdditionalQuestion2);
        }
            
    }
}
