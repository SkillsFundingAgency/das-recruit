using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace UnitTests.Qa.Web.Orchestrators.Review
{
    public class SubmitReviewOrchestratorTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task When_Submitting_If_There_Manual_Edited_Fields_They_Are_Marked_And_Updated(
            ReviewEditModel editModel,
            VacancyUser user,
            Vacancy vacancy,
            [Frozen] Mock<IQaVacancyClient> qaVacancyClient,
            ReviewOrchestrator orchestrator)
        {
            //Arrange
            editModel.SelectedFieldIdentifiers = new List<string>();
            editModel.SelectedAutomatedQaResults = new List<string>();
            var vacancyReview = new VacancyReview
            {
                Id = editModel.ReviewId,
                VacancyReference = vacancy.VacancyReference.Value,
                VacancySnapshot = vacancy
            };
            qaVacancyClient.Setup(x => x.GetVacancyReviewAsync(editModel.ReviewId)).ReturnsAsync(vacancyReview);
            qaVacancyClient.Setup(x => x.GetAssignedVacancyReviewsForUserAsync(user.UserId)).ReturnsAsync(
                new List<VacancyReview>
                {
                    vacancyReview
                });
            qaVacancyClient.Setup(x => x.GetVacancyAsync(vacancyReview.VacancyReference)).ReturnsAsync(vacancy);
            
            //Act
            await orchestrator.SubmitReviewAsync(editModel, user);

            //Assert
            qaVacancyClient.Verify(x=>x.UpdateDraftVacancyAsync(It.Is<Vacancy>(c=>
                c.OutcomeDescription == editModel.OutcomeDescription
                && c.TrainingDescription == editModel.TrainingDescription
                && c.AdditionalTrainingDescription == editModel.AdditionalTrainingDescription
                && c.Wage.WorkingWeekDescription == editModel.WorkingWeekDescription
                && c.Wage.CompanyBenefitsInformation == editModel.CompanyBenefitsInformation
                && c.ShortDescription == editModel.ShortDescription
                && c.Description == editModel.VacancyDescription
                ), user), Times.Once);
        }
        
        [Test, RecursiveMoqAutoData]
        public async Task When_Submitting_If_There_Manual_Edited_Fields_They_Are_Sent_On_The_ApproveVacancyReviewCommand(
            ReviewEditModel editModel,
            VacancyUser user,
            Vacancy vacancy,
            [Frozen] Mock<IQaVacancyClient> qaVacancyClient,
            [Frozen] Mock<IMessaging> messaging,
            ReviewOrchestrator orchestrator)
        {
            //Arrange
            editModel.SelectedFieldIdentifiers = new List<string>();
            editModel.SelectedAutomatedQaResults = new List<string>();
            var vacancyReview = new VacancyReview
            {
                Id = editModel.ReviewId,
                VacancyReference = vacancy.VacancyReference.Value,
                VacancySnapshot = vacancy
            };
            qaVacancyClient.Setup(x => x.GetVacancyReviewAsync(editModel.ReviewId)).ReturnsAsync(vacancyReview);
            qaVacancyClient.Setup(x => x.GetAssignedVacancyReviewsForUserAsync(user.UserId)).ReturnsAsync(
                new List<VacancyReview>
                {
                    vacancyReview
                });
            qaVacancyClient.Setup(x => x.GetVacancyAsync(vacancyReview.VacancyReference)).ReturnsAsync(vacancy);
            
            //Act
            await orchestrator.SubmitReviewAsync(editModel, user);

            //Assert
            messaging.Verify(x=>x.SendCommandAsync(It.Is<ApproveVacancyReviewCommand>(c=>
                c.ReviewId.Equals(editModel.ReviewId)
                && c.ManualQaFieldEditIndicators.SingleOrDefault(x=>x.FieldIdentifier.Equals(nameof(editModel.OutcomeDescription))) != null
                && c.ManualQaFieldEditIndicators.SingleOrDefault(x=>x.FieldIdentifier.Equals(nameof(editModel.TrainingDescription))) != null
                && c.ManualQaFieldEditIndicators.SingleOrDefault(x=>x.FieldIdentifier.Equals(nameof(editModel.AdditionalTrainingDescription))) != null
                && c.ManualQaFieldEditIndicators.SingleOrDefault(x=>x.FieldIdentifier.Equals(nameof(editModel.ShortDescription))) != null
                && c.ManualQaFieldEditIndicators.SingleOrDefault(x=>x.FieldIdentifier.Equals(nameof(editModel.VacancyDescription))) != null
                && c.ManualQaFieldEditIndicators.SingleOrDefault(x=>x.FieldIdentifier.Equals(nameof(editModel.WorkingWeekDescription))) != null
                && c.ManualQaFieldEditIndicators.SingleOrDefault(x=>x.FieldIdentifier.Equals(nameof(editModel.CompanyBenefitsInformation))) != null
                )), Times.Once);
            
        }
    }
}