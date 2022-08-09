using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.VacancyDescription;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Mappings
{
    public class GetFutureProspectsFieldIndicatorsTests
    {
        [Test]
        public void When_Calling_GetFutureProspectsFieldIndicators_Then_Returns_Correct_Field_Indicators()
        {
            var expectedFieldIdentifiers = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.OutcomeDescription, nameof(VacancyDescriptionEditModel.OutcomeDescription))
            };
            var expectedMappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.OutcomeDescription), new []{ FieldIdentifiers.OutcomeDescription} }
            };
            
            var result = ReviewFieldMappingLookups.GetFutureProspectsFieldIndicators();

            result.FieldIdentifiersForPage.Should().BeEquivalentTo(expectedFieldIdentifiers);
            result.VacancyPropertyMappingsLookup.Should().BeEquivalentTo(expectedMappings);
        }
    }
}