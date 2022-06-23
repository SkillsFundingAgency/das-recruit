using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateEmployerDashboardOnChangeTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_Dashboard_Is_Not_Rebuilt_For_Employers_When_Traineeship_Vacancy(
            ApplicationReviewCreatedEvent applicationReviewCreatedEvent,
            Vacancy vacancy,
            [Frozen] Mock<IVacancyRepository> vacancyRepository,
            [Frozen] Mock<IEmployerDashboardProjectionService> employerDashboardService,
            UpdateEmployerDashboardOnChange handler)
        {
            //Arrange
            vacancy.VacancyReference = applicationReviewCreatedEvent.VacancyReference;
            vacancy.VacancyType = VacancyType.Traineeship;
            vacancyRepository.Setup(x => x.GetVacancyAsync(applicationReviewCreatedEvent.VacancyReference))
                .ReturnsAsync(vacancy);
            
            //Act
            await handler.Handle(applicationReviewCreatedEvent, CancellationToken.None);

            //Assert
            employerDashboardService.Verify(x=>x.ReBuildDashboardAsync(It.IsAny<string>()), Times.Never);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Dashboard_Is_Rebuilt_For_Employers_When_Not_Traineeship_Vacancy(
            ApplicationReviewCreatedEvent applicationReviewCreatedEvent,
            Vacancy vacancy,
            [Frozen] Mock<IVacancyRepository> vacancyRepository,
            [Frozen] Mock<IEmployerDashboardProjectionService> employerDashboardService,
            UpdateEmployerDashboardOnChange handler)
        {
            //Arrange
            vacancy.VacancyReference = applicationReviewCreatedEvent.VacancyReference;
            vacancy.VacancyType = null;
            vacancyRepository.Setup(x => x.GetVacancyAsync(applicationReviewCreatedEvent.VacancyReference))
                .ReturnsAsync(vacancy);
            
            //Act
            await handler.Handle(applicationReviewCreatedEvent, CancellationToken.None);

            //Assert
            employerDashboardService.Verify(x=>x.ReBuildDashboardAsync(vacancy.EmployerAccountId), Times.Once);
        }
    }
}