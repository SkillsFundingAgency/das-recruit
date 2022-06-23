using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels.EditVacancyDates;
using Esfa.Recruit.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.EditVacancyDatesOrchestratorTests
{
    public class WhenPostingEditVacancyDatesModel
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.TrainingProgramme | VacancyRuleSet.StartDateEndDate | VacancyRuleSet.TrainingExpiryDate | VacancyRuleSet.MinimumWage;
        [Test, MoqAutoData]
        public async Task Then_The_Vacancy_Is_Updated_With_The_New_Dates(
            EditVacancyDatesEditModel model,
            VacancyUser user,
            Vacancy vacancy,
            [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
            [Frozen] Mock<IUtility> utility,
            EditVacancyDatesOrchestrator orchestrator)
        {
            var proposedClosingDate = DateTime.UtcNow.AddMonths(4);
            var proposedStartDate = DateTime.UtcNow.AddMonths(3);
            var updateKind = LiveUpdateKind.ClosingDate;
            updateKind |= LiveUpdateKind.StartDate;
            model.ClosingDay = proposedClosingDate.Day.ToString();
            model.ClosingMonth = proposedClosingDate.Month.ToString();
            model.ClosingYear = proposedClosingDate.Year.ToString();
            model.StartDay = proposedStartDate.Day.ToString();
            model.StartMonth = proposedStartDate.Month.ToString();
            model.StartYear = proposedStartDate.Year.ToString();
            vacancy.ClosingDate = DateTime.UtcNow.AddMonths(1);
            vacancy.StartDate = DateTime.UtcNow.AddMonths(2);
            vacancy.Status = VacancyStatus.Live;
            vacancy.IsDeleted = false;
            DateTime.TryParseExact(model.StartDate, "d/M/yyyy", new CultureInfo("en-GB"),
                DateTimeStyles.AssumeUniversal, out var expectedStartDate);
            DateTime.TryParseExact(model.ClosingDate, "d/M/yyyy", new CultureInfo("en-GB"),
                DateTimeStyles.AssumeUniversal, out var expectedClosingDate);
            vacancyClient.Setup(x => x.GetVacancyAsync(model.VacancyId.GetValueOrDefault()))
                .ReturnsAsync(vacancy);
            vacancyClient.Setup(x => x.Validate(vacancy, ValidationRules)).Returns(new EntityValidationResult{Errors = new List<EntityValidationError>()});
            
            var actual = await orchestrator.PostEditVacancyDatesEditModelAsync(model, user);

            actual.Success.Should().BeTrue();
            vacancyClient.Verify(x=>x.UpdatePublishedVacancyAsync(It.Is<Vacancy>(c=>
                c.StartDate.Value.ToString("d/M/yyyy") == expectedStartDate.ToString("d/M/yyyy")
                && c.ClosingDate.Value.ToString("d/M/yyyy") == expectedClosingDate.ToString("d/M/yyyy")
            ), user, updateKind), Times.Once);
        }
    }
}