﻿using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.AdditionalQuestions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Mappings;

public class GetAdditionalQuestionsFieldIndicatorsTests
{
    [Test]
    public void When_Calling_GetAdditionalQuestionsFieldIndicators_Then_Returns_Correct_Field_Indicators()
    {
        var expectedFieldIdentifiers = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.AdditionalQuestion1, nameof(AdditionalQuestionsEditModel.AdditionalQuestion1)),
            new(FieldIdentifiers.AdditionalQuestion2, nameof(AdditionalQuestionsEditModel.AdditionalQuestion2))
        };
        var expectedMappings =  new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.AdditionalQuestion1), new []{ FieldIdentifiers.AdditionalQuestion1} },
            { FieldIdResolver.ToFieldId(v => v.AdditionalQuestion2), new []{ FieldIdentifiers.AdditionalQuestion2} }
        };
            
        var result = ReviewFieldMappingLookups.GetAdditionalQuestionsFieldIndicators();

        result.FieldIdentifiersForPage.Should().BeEquivalentTo(expectedFieldIdentifiers);
        result.VacancyPropertyMappingsLookup.Should().BeEquivalentTo(expectedMappings);
    }
}