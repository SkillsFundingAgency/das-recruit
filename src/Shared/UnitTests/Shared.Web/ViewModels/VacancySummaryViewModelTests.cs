using AutoFixture.NUnit3;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.UnitTests.Shared.Web.ViewModels
{
    public class VacancySummaryViewModelTests
    {
        [Test]
        [InlineAutoData(VacancyStatus.Live, true)]
        [InlineAutoData(VacancyStatus.Closed, true)]
        [InlineAutoData(VacancyStatus.Draft, false)]
        [InlineAutoData(VacancyStatus.Referred, false)]
        [InlineAutoData(VacancyStatus.Rejected, false)]
        [InlineAutoData(VacancyStatus.Review, false)]
        [InlineAutoData(VacancyStatus.Approved, false)]
        public void Then_Can_Show_Vacancy_Count_For_Apprenticeships(VacancyStatus status, bool expected, VacancySummaryViewModel model)
        {
            //Arrange Act
            model.Status = status;
            model.ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship;

            //Assert
            model.CanShowVacancyApplicationsCount.Should().Be(expected);
        }
        
        [Test]
        [InlineAutoData(VacancyStatus.Live, true)]
        [InlineAutoData(VacancyStatus.Closed, true)]
        [InlineAutoData(VacancyStatus.Draft, false)]
        [InlineAutoData(VacancyStatus.Referred, false)]
        [InlineAutoData(VacancyStatus.Rejected, false)]
        [InlineAutoData(VacancyStatus.Review, false)]
        [InlineAutoData(VacancyStatus.Approved, false)]
        public void Then_Can_Show_Vacancy_Count_For_Traineeships(VacancyStatus status, bool expected, VacancySummaryViewModel model)
        {
            //Arrange Act
            model.Status = status;
            model.ApplicationMethod = ApplicationMethod.ThroughFindATraineeship;

            //Assert
            model.CanShowVacancyApplicationsCount.Should().Be(expected);

        }

        [Test, AutoData]
        public void Then_Can_Show_Vacancy_Count_False_When_Not_Through_Faa_or_Fat(VacancySummaryViewModel model)
        {
            //Arrange Act
            model.Status = VacancyStatus.Live;
            model.ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite;
            
            //Assert
            model.CanShowVacancyApplicationsCount.Should().BeFalse();
        }
    }
}