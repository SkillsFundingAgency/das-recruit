using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels.DeleteVacancy;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.DeleteVacancyOrchestratorTests
{
    class WhenPostingDeleteVacancy
    {

        [Test, MoqAutoData]
        public async Task ThenReturnsViewModelWit_VacancyReference_Title_Status(
                 DeleteEditModel model,
                 VacancyUser user,
                 Vacancy vacancy,
                 [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
                 DeleteVacancyOrchestrator orchestrator)
        {
            vacancy.ClosingDate = DateTime.UtcNow.AddMonths(-1);
            vacancy.StartDate = DateTime.UtcNow.AddMonths(-2);
            vacancy.Status = VacancyStatus.Submitted;
            vacancy.IsDeleted = false;

            vacancyClient.Setup(x => x.GetVacancyAsync(model.VacancyId.GetValueOrDefault()))
                .ReturnsAsync(vacancy);

            var actual = await orchestrator.DeleteVacancyAsync(model, user);

            actual.VacancyReference.Should().Be(vacancy.VacancyReference);
            actual.Title.Should().Be(vacancy.Title);
            actual.Status.Should().Be(vacancy.Status);
        }
    }
}
