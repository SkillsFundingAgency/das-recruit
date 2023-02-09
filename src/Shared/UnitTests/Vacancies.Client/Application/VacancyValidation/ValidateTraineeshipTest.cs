using System;
using System.Collections.Generic;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation
{
    public class ValidateTraineeshipTest : VacancyValidationTestsBase
    {
        public ValidateTraineeshipTest()
        {
            ServiceParameters = new ServiceParameters("Traineeship");
        }

        [Fact]
        public void Then_Does_Not_Validate_Qualifications_TrainingProgramme_And_Wage()
        {
            
            var fixture = new Fixture();
            var vacancy = fixture.Build<Vacancy>().Create();
            vacancy.Title = "Traineeship vacancy";
            vacancy.Qualifications = null;
            vacancy.Wage = null;
            vacancy.StartDate = DateTime.UtcNow.AddMonths(3);
            vacancy.ClosingDate = DateTime.UtcNow.AddMonths(2);
            vacancy.Skills = new List<string> {"Skill 1"};
            vacancy.ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship;
            vacancy.ApplicationUrl = null;
            vacancy.ApplicationInstructions = string.Empty;
            vacancy.EmployerContact = new ContactDetail();
            vacancy.EmployerWebsiteUrl = null;
            vacancy.ProviderContact = new ContactDetail();
            vacancy.TrainingProvider.Ukprn = 10000001;
            vacancy.EmployerLocation.Postcode = "CV1 1WS";
            vacancy.ProgrammeId = null;
            MockProviderRelationshipsService.Setup(p => 
                    p.HasProviderGotEmployersPermissionAsync(vacancy.TrainingProvider.Ukprn.Value, vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId, OperationType.Recruitment))
                .ReturnsAsync(true);
            MockTrainingProviderSummaryProvider.Setup(x => x.GetAsync(vacancy.TrainingProvider.Ukprn.Value))
                .ReturnsAsync(new TrainingProviderSummary());

            var result = Validator.Validate(vacancy, VacancyRuleSet.All);
            
            result.HasErrors.Should().BeFalse();
            
        }
    }
}