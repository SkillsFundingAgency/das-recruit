using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Dates;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Mappings
{
    public class GetDatesReviewFieldIndicatorsTests
    {
        [Test]
        public void When_Calling_GetDatesReviewFieldIndicators_Then_Returns_Correct_Field_Indicators()
        {
            var expectedFieldIdentifiers = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ClosingDate, nameof(DatesEditModel.ClosingDay)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.PossibleStartDate, nameof(DatesEditModel.StartDay))
            };
            var expectedMappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.ClosingDate), new []{ FieldIdentifiers.ClosingDate} },
                { FieldIdResolver.ToFieldId(v => v.StartDate), new []{ FieldIdentifiers.PossibleStartDate} }
            };
            
            var result = ReviewFieldMappingLookups.GetDatesReviewFieldIndicators();

            result.FieldIdentifiersForPage.Should().BeEquivalentTo(expectedFieldIdentifiers);
            result.VacancyPropertyMappingsLookup.Should().BeEquivalentTo(expectedMappings);
        }
    }
}