using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.VacancyDescription;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Mappings
{
    public class GetAboutEmployerFieldIndicatorsTests
    {
        [Test]
        public void When_Calling_GetAboutEmployerFieldIndicators_Then_Returns_Correct_Field_Indicators()
        {
            var expectedFieldIdentifiers = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerDescription, nameof(AboutEmployerEditModel.EmployerDescription)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerWebsiteUrl, nameof(AboutEmployerEditModel.EmployerWebsiteUrl)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.DisabilityConfident, nameof(AboutEmployerEditModel.IsDisabilityConfident))
            };
            var expectedMappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.EmployerDescription), new [] { FieldIdentifiers.EmployerDescription }},
                { FieldIdResolver.ToFieldId(v => v.EmployerWebsiteUrl), new []{ FieldIdentifiers.EmployerWebsiteUrl} },
                { FieldIdResolver.ToFieldId(v => v.DisabilityConfident), new []{ FieldIdentifiers.DisabilityConfident} }
            };
            
            var result = ReviewFieldMappingLookups.GetAboutEmployerFieldIndicators();

            result.FieldIdentifiersForPage.Should().BeEquivalentTo(expectedFieldIdentifiers);
            result.VacancyPropertyMappingsLookup.Should().BeEquivalentTo(expectedMappings);
        }
    }
}