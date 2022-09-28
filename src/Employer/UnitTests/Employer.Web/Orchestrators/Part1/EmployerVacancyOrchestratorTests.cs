using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part1
{
    public class EmployerVacancyOrchestratorTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_Vacancy_Total_Is_Retrieved_And_Set_To_True_When_Has_Vacancies(
            long vacancyCount,
            string employerAccountId,
            [Frozen] Mock<IEmployerVacancyClient> vacancyClient,
            EmployerVacancyOrchestrator orchestrator)
        {
            vacancyClient.Setup(x => x.GetVacancyCount(employerAccountId,VacancyType.Apprenticeship,null, null))
                .ReturnsAsync(vacancyCount);
            
            var hasNoVacancies = await orchestrator.HasNoVacancies(employerAccountId);
            
            hasNoVacancies.Should().BeFalse();
        }
        [Test, MoqAutoData]
        public async Task Then_The_Vacancy_Total_Is_Retrieved_And_Set_To_False_When_Has_No_Vacancies(
            string employerAccountId,
            [Frozen] Mock<IEmployerVacancyClient> vacancyClient,
            EmployerVacancyOrchestrator orchestrator)
        {
            vacancyClient.Setup(x => x.GetVacancyCount(employerAccountId,VacancyType.Apprenticeship,null, null))
                .ReturnsAsync(0);
            
            var hasNoVacancies = await orchestrator.HasNoVacancies(employerAccountId);
            
            hasNoVacancies.Should().BeTrue();
        }
    }
}