using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Vacancies.Client.Application.Providers;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators
{
    public class VacancyPreviewOrchestratorTests
    {
        [MoqInlineAutoData(true, true)]
        [MoqInlineAutoData(false, false)]         
        public async Task ApproveJobAdvertAsync_ShouldNotSubmitWhenMissingAgreements(
            bool hasLegalEntityAgreement,
            bool shouldBeSubmitted,
            Mock<IRecruitVacancyClient> mockRecruitVacancyClient,
            Mock<IReviewSummaryService> mockReviewSummaryService,
            Mock<IMessaging> mockmessaging)
        {
            //Arrange            
            var user =  new VacancyUser {Email = "advert@advert.com", Name = "advertname",  UserId = "advertId" };

            var vacancy = new Vacancy
            {
                EmployerAccountId = "ABCDEF",
                AccountLegalEntityPublicHashedId = "XVYABD",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                NumberOfPositions = 1,
                EmployerLocation = new Address { Postcode = "has a value" },
                ShortDescription = "has a value",
                ProgrammeId = "has a value",
                Wage = new Wage { Duration = 1, WageType = WageType.FixedWage },
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                StartDate = DateTime.Now,
                Status = VacancyStatus.Review
            };

            var approveJobAdvertViewModel = new ApproveJobAdvertViewModel { ApproveJobAdvert = true, VacancyId = vacancy.Id, EmployerAccountId = "ABCDEF" };

            mockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(vacancy.Id)).ReturnsAsync(vacancy);
            mockRecruitVacancyClient.Setup(x => x.GetEmployerDescriptionAsync(vacancy)).ReturnsAsync("employer description");
            mockRecruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync("employer name");
            mockRecruitVacancyClient.Setup(c => c.Validate(vacancy, It.IsAny<VacancyRuleSet>())).Returns(new EntityValidationResult());

            var geocodeImageService = new Mock<IGeocodeImageService>();
            var externalLinks = new Mock<IOptions<ExternalLinksConfiguration>>();
            var mapper = new DisplayVacancyViewModelMapper(geocodeImageService.Object, externalLinks.Object, mockRecruitVacancyClient.Object, Mock.Of<IApprenticeshipProgrammeProvider>());

            var legalEntityAgreement = new Mock<ILegalEntityAgreementService>();
            legalEntityAgreement.Setup(l => l.HasLegalEntityAgreementAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId))
                .ReturnsAsync(hasLegalEntityAgreement);
            var utility = new Utility(mockRecruitVacancyClient.Object);
            
            var sut = new VacancyPreviewOrchestrator(mockRecruitVacancyClient.Object,
                                                    Mock.Of<ILogger<VacancyPreviewOrchestrator>>(), mapper,
                                                    mockReviewSummaryService.Object, legalEntityAgreement.Object,
                                                    mockmessaging.Object,
                                                    Mock.Of<IOptions<ExternalLinksConfiguration>>(), utility);


            //Act
            var response = await sut.ApproveJobAdvertAsync(approveJobAdvertViewModel, user);

            //Assert           
            var submittedTimes = shouldBeSubmitted ? Times.Once() : Times.Never();
            mockmessaging.Verify(c => c.SendCommandAsync(It.IsAny<ICommand>()), submittedTimes);
            response.Data.IsSubmitted.Should().Be(shouldBeSubmitted);
            response.Data.HasLegalEntityAgreement.Should().Be(hasLegalEntityAgreement);
        }

        [Test, MoqAutoData]
        public async Task RejectJobAdvertAsync_ShouldSendCommand(
            Mock<IRecruitVacancyClient> mockRecruitVacancyClient,
            Mock<IReviewSummaryService> mockReviewSummaryService,
            Mock<IMessaging> mockmessaging)
        {
            //Arrange            
            var user = new VacancyUser { Email = "advert@advert.com", Name = "advertname", UserId = "advertId" };

            var vacancy = new Vacancy
            {
                EmployerAccountId = "ABCDEF",
                AccountLegalEntityPublicHashedId = "XVYABD",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                NumberOfPositions = 1,
                EmployerLocation = new Address { Postcode = "has a value" },
                ShortDescription = "has a value",
                ProgrammeId = "has a value",
                Wage = new Wage { Duration = 1, WageType = WageType.FixedWage },
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                StartDate = DateTime.Now,
                Status = VacancyStatus.Review,
                VacancyReference = 123456
            };

            var rejectJobAdvertViewModel = new RejectJobAdvertViewModel { RejectJobAdvert = true, VacancyId = vacancy.Id, EmployerAccountId = "ABCDEF" };

            mockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(vacancy.Id)).ReturnsAsync(vacancy);
            mockRecruitVacancyClient.Setup(x => x.GetEmployerDescriptionAsync(vacancy)).ReturnsAsync("employer description");
            mockRecruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync("employer name");
            mockRecruitVacancyClient.Setup(c => c.Validate(vacancy, It.IsAny<VacancyRuleSet>())).Returns(new EntityValidationResult());

            var geocodeImageService = new Mock<IGeocodeImageService>();
            var externalLinks = new Mock<IOptions<ExternalLinksConfiguration>>();
            var mapper = new DisplayVacancyViewModelMapper(geocodeImageService.Object, externalLinks.Object, mockRecruitVacancyClient.Object,Mock.Of<IApprenticeshipProgrammeProvider>());
            var legalEntityAgreement = new Mock<ILegalEntityAgreementService>();            
            var utility = new Utility(mockRecruitVacancyClient.Object);
            
            var sut = new VacancyPreviewOrchestrator(mockRecruitVacancyClient.Object,
                                                    Mock.Of<ILogger<VacancyPreviewOrchestrator>>(), mapper,
                                                    mockReviewSummaryService.Object, legalEntityAgreement.Object,
                                                    mockmessaging.Object,
                                                    Mock.Of<IOptions<ExternalLinksConfiguration>>(), utility);

            //Act
            var response = await sut.RejectJobAdvertAsync(rejectJobAdvertViewModel, user);

            //Assert           
            mockmessaging.Verify(c => c.SendCommandAsync(It.IsAny<ICommand>()), Times.Once());
            response.Data.IsRejected.Should().Be(true);            
        }
    }
}