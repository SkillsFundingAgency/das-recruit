using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Duration;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Address = Esfa.Recruit.Vacancies.Client.Domain.Entities.Address;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part1
{
    public class LocationOrchestratorTests
    {
        private LocationOrchestratorTestsFixture _fixture;

        public LocationOrchestratorTests()
        {
            _fixture = new LocationOrchestratorTestsFixture();
        }

        [Theory]
        [InlineData("this is a value", "this is a value", "this is a value", "this is a value", "this is a value", false)]
        [InlineData("this is a new value", "this is a value", "this is a value", "this is a value", "this is a value", true)]
        [InlineData("this is a value", "this is a new value", "this is a value", "this is a value", "this is a value", true)]
        [InlineData("this is a value", "this is a value", "this is a new value", "this is a value", "this is a value", true)]
        [InlineData("this is a value", "this is a value", "this is a value", "this is a new value", "this is a value", true)]
        [InlineData("this is a value", "this is a value", "this is a value", "this is a value", "this is a new value", true)]
        [InlineData("this is a new value", "this is a new value", "this is a new value", "this is a new value", "this is a new value", true)]
        public async Task WhenAddressUpdated_ShouldFlagFieldIndicators(string addressLine1, string addressLine2, string addressLine3, string addressLine4, string postcode, bool fieldIndicatorSet)
        {
            _fixture
                .WithAddressLine1("this is a value")
                .WithAddressLine2("this is a value")
                .WithAddresLine3("this is a value")
                .WithAddresLine4("this is a value")
                .WithPostcode("this is a value")
                .Setup();

            var locationEditModel = new LocationEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                AddressLine1 = addressLine1,
                AddressLine2 = addressLine2,
                AddressLine3 = addressLine3,
                AddressLine4 = addressLine4,
                Postcode = postcode,
                SelectedLocation = LocationViewModel.UseOtherLocationConst
            };

            await _fixture.PostLocationEditModelAsync(locationEditModel);

            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.EmployerAddress, fieldIndicatorSet);
        }

        [Fact]
        public async Task WhenEmployerLegalEntityUpdated_ShouldFlagEmployerNameFieldIndicator()
        {
            _fixture
                .WithEmployerNameOption(EmployerNameOption.RegisteredName)
                .Setup();

            var locationEditModel = new LocationEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                SelectedLocation = LocationViewModel.UseOtherLocationConst
            };

            await _fixture.PostLocationEditModelAsync(locationEditModel, new VacancyEmployerInfoModel
            {
                EmployerIdentityOption = EmployerIdentityOption.RegisteredName,
                AccountLegalEntityPublicHashedId = VacancyOrchestratorTestData.AccountLegalEntityPublicHashedId456
            });

            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.EmployerName, true);
        }

        [Theory]
        [InlineData(EmployerNameOption.RegisteredName, EmployerIdentityOption.ExistingTradingName, null, true)]
        [InlineData(EmployerNameOption.RegisteredName, EmployerIdentityOption.NewTradingName, "this is a new value", true)]
        [InlineData(EmployerNameOption.TradingName, EmployerIdentityOption.NewTradingName, "this is a new value", true)]
        [InlineData(EmployerNameOption.TradingName, EmployerIdentityOption.ExistingTradingName, null, false)]
        [InlineData(EmployerNameOption.RegisteredName, EmployerIdentityOption.RegisteredName, null, false)]
        public async Task WhenEmployerNameOptionUpdated_ShouldFlagEmployerNameFieldIndicator(EmployerNameOption employerNameOption, EmployerIdentityOption employerIdentityOption, string newTradingName, bool shouldFlagIndicator)
        {
            _fixture
                .WithEmployerNameOption(employerNameOption)
                .WithTradingName(employerNameOption == EmployerNameOption.TradingName
                    ? "this is a value"
                    : null)
                .Setup();

            var locationEditModel = new LocationEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                SelectedLocation = LocationViewModel.UseOtherLocationConst
            };

            await _fixture.PostLocationEditModelAsync(locationEditModel, new VacancyEmployerInfoModel
            {
                EmployerIdentityOption = employerIdentityOption,
                NewTradingName = newTradingName,
                AccountLegalEntityPublicHashedId = _fixture.Vacancy.AccountLegalEntityPublicHashedId
            });

            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.EmployerName, shouldFlagIndicator);
        }

        public class LocationOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerAddress;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public EmployerEditVacancyInfo EmployerEditVacancyInfo { get; }
            public EmployerProfile VacancyEmployerProfile { get;  }
            public EmployerProfile AlternateEmployerProfile { get;  }
            public LocationOrchestrator Sut {get; private set;}

            public LocationOrchestratorTestsFixture()
            {
                MockClient = new Mock<IEmployerVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
                VacancyEmployerProfile = VacancyOrchestratorTestData.GetEmployerProfile(Vacancy.AccountLegalEntityPublicHashedId);
                AlternateEmployerProfile = VacancyOrchestratorTestData.GetEmployerProfile(VacancyOrchestratorTestData.AccountLegalEntityPublicHashedId456);
                EmployerEditVacancyInfo = VacancyOrchestratorTestData.GetEmployerEditVacancyInfo();
            }

            public LocationOrchestratorTestsFixture WithAddressLine1(string addressLine1)
            {
                if (Vacancy.EmployerLocation == null)
                    Vacancy.EmployerLocation = new Address();

                Vacancy.EmployerLocation.AddressLine1 = addressLine1;
                return this;
            }

            public LocationOrchestratorTestsFixture WithAddressLine2(string addressLine2)
            {
                if (Vacancy.EmployerLocation == null)
                    Vacancy.EmployerLocation = new Address();

                Vacancy.EmployerLocation.AddressLine2 = addressLine2;
                return this;
            }

            public LocationOrchestratorTestsFixture WithAddresLine3(string addressLine3)
            {
                if (Vacancy.EmployerLocation == null)
                    Vacancy.EmployerLocation = new Address();

                Vacancy.EmployerLocation.AddressLine3 = addressLine3;
                return this;
            }

            public LocationOrchestratorTestsFixture WithAddresLine4(string addressLine4)
            {
                if (Vacancy.EmployerLocation == null)
                    Vacancy.EmployerLocation = new Address();

                Vacancy.EmployerLocation.AddressLine4 = addressLine4;
                return this;
            }

            public LocationOrchestratorTestsFixture WithPostcode(string postcode)
            {
                if (Vacancy.EmployerLocation == null)
                    Vacancy.EmployerLocation = new Address();

                Vacancy.EmployerLocation.Postcode = postcode;
                return this;
            }

            public LocationOrchestratorTestsFixture WithEmployerNameOption(EmployerNameOption employerNameOption)
            {
                Vacancy.EmployerNameOption = employerNameOption;
                return this;
            }

            public LocationOrchestratorTestsFixture WithTradingName(string tradingName)
            {
                VacancyEmployerProfile.TradingName = tradingName;
                return this;
            }

            public void Setup()
            {
                MockClient.Setup(x => x.GetEditVacancyInfoAsync(Vacancy.EmployerAccountId)).ReturnsAsync(EmployerEditVacancyInfo);
                
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.GetEmployerProfileAsync(Vacancy.EmployerAccountId, Vacancy.AccountLegalEntityPublicHashedId)).ReturnsAsync(VacancyEmployerProfile);
                MockRecruitVacancyClient.Setup(x => x.GetEmployerProfileAsync(Vacancy.EmployerAccountId, AlternateEmployerProfile.AccountLegalEntityPublicHashedId)).ReturnsAsync(AlternateEmployerProfile);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

                Sut = new LocationOrchestrator(MockClient.Object, MockRecruitVacancyClient.Object, Mock.Of<ILogger<LocationOrchestrator>>(), 
                    Mock.Of<IReviewSummaryService>());
            }

            public async Task PostLocationEditModelAsync(LocationEditModel model, VacancyEmployerInfoModel vacancyEmployerInfoModel = null)
            {
                await Sut.PostLocationEditModelAsync(model,
                    vacancyEmployerInfoModel ?? new VacancyEmployerInfoModel
                    {
                        VacancyId = Vacancy.Id
                    }, 
                    User);
            }

            public void VerifyEmployerReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.EmployerReviewFieldIndicators
                    .Where(p => p.FieldIdentifier == fieldIdentifier).Single()
                    .Should().NotBeNull().And
                    .Match<EmployerReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public Mock<IEmployerVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
