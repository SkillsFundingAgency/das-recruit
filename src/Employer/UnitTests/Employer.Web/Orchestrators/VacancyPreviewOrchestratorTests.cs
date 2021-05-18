using System;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators
{
    public class VacancyPreviewOrchestratorTests
    {
        private readonly Mock<IEmployerVacancyClient> _mockEmployerVacancyClient;
        private readonly Mock<IRecruitVacancyClient> _mockRecruitVacancyClient;
        private readonly Mock<IReviewSummaryService> _mockReviewSummaryService;
        private readonly Mock<ILegalEntityAgreementService> _mockLegalEntityAgreementService;
        private readonly Mock<IMessaging> _mockmessaging;

        public VacancyPreviewOrchestratorTests()
        {
            _mockEmployerVacancyClient = new Mock<IEmployerVacancyClient>();
            _mockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();
            _mockReviewSummaryService = new Mock<IReviewSummaryService>();
            _mockLegalEntityAgreementService = new Mock<ILegalEntityAgreementService>();
            _mockmessaging = new Mock<IMessaging>();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]         
        public async Task ApproveJobAdvertAsync_ShouldNotSubmitWhenMissingAgreements(
            bool hasLegalEntityAgreement, bool shouldBeSubmitted)
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

            _mockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(vacancy.Id)).ReturnsAsync(vacancy);
            _mockRecruitVacancyClient.Setup(x => x.GetEmployerDescriptionAsync(vacancy)).ReturnsAsync("employer description");
            _mockRecruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync("employer name");
            _mockRecruitVacancyClient.Setup(c => c.Validate(vacancy, It.IsAny<VacancyRuleSet>())).Returns(new EntityValidationResult());

            var geocodeImageService = new Mock<IGeocodeImageService>();
            var externalLinks = new Mock<IOptions<ExternalLinksConfiguration>>();
            var mapper = new DisplayVacancyViewModelMapper(geocodeImageService.Object, externalLinks.Object, _mockRecruitVacancyClient.Object);

            var legalEntityAgreement = new Mock<ILegalEntityAgreementService>();
            legalEntityAgreement.Setup(l => l.HasLegalEntityAgreementAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId))
                .ReturnsAsync(hasLegalEntityAgreement);

            var sut = new VacancyPreviewOrchestrator(_mockEmployerVacancyClient.Object,
                                                    _mockRecruitVacancyClient.Object,
                                                    Mock.Of<ILogger<VacancyPreviewOrchestrator>>(), mapper,
                                                    _mockReviewSummaryService.Object, legalEntityAgreement.Object,
                                                    _mockmessaging.Object);


            //Act
            var response = await sut.ApproveJobAdvertAsync(approveJobAdvertViewModel, user);

            //Assert           
            var submittedTimes = shouldBeSubmitted ? Times.Once() : Times.Never();
            _mockmessaging.Verify(c => c.SendCommandAsync(It.IsAny<ICommand>()), submittedTimes);
            response.Data.IsSubmitted.Should().Be(shouldBeSubmitted);
            response.Data.HasLegalEntityAgreement.Should().Be(hasLegalEntityAgreement);
        }

        [Fact]
        public async Task RejectJobAdvertAsync_ShouldSendCommand()
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

            _mockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(vacancy.Id)).ReturnsAsync(vacancy);
            _mockRecruitVacancyClient.Setup(x => x.GetEmployerDescriptionAsync(vacancy)).ReturnsAsync("employer description");
            _mockRecruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync("employer name");
            _mockRecruitVacancyClient.Setup(c => c.Validate(vacancy, It.IsAny<VacancyRuleSet>())).Returns(new EntityValidationResult());

            var geocodeImageService = new Mock<IGeocodeImageService>();
            var externalLinks = new Mock<IOptions<ExternalLinksConfiguration>>();
            var mapper = new DisplayVacancyViewModelMapper(geocodeImageService.Object, externalLinks.Object, _mockRecruitVacancyClient.Object);
            var shouldBeRejected = true;
            var legalEntityAgreement = new Mock<ILegalEntityAgreementService>();            

            var sut = new VacancyPreviewOrchestrator(_mockEmployerVacancyClient.Object,
                                                    _mockRecruitVacancyClient.Object,
                                                    Mock.Of<ILogger<VacancyPreviewOrchestrator>>(), mapper,
                                                    _mockReviewSummaryService.Object, legalEntityAgreement.Object,
                                                    _mockmessaging.Object);


            //Act
            var response = await sut.RejectJobAdvertAsync(rejectJobAdvertViewModel, user);

            //Assert           
            var rejectedTime = shouldBeRejected ? Times.Once() : Times.Never();
            _mockmessaging.Verify(c => c.SendCommandAsync(It.IsAny<ICommand>()), rejectedTime);
            response.Data.IsRejected.Should().Be(shouldBeRejected);            
        }
    }
}
