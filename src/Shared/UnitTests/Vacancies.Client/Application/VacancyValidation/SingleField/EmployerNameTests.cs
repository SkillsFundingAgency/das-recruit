using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class EmployerNameTests : VacancyValidationTestsBase
    {
        [Fact]
        public void RegisteredName_ShouldValidateEmpty()
        {
            var vacancy = new Vacancy() {
                EmployerName = string.Empty
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerName);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerName));
            result.Errors[0].ErrorCode.Should().Be("4");
        }

        [Fact]
        public void TradingName_ShouldValidateEmpty()
        {
            var vacancy = new Vacancy() {
                EmployerName = string.Empty,
                EmployerNameOption = EmployerNameOption.TradingName
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TradingName);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerName));
            result.Errors[0].ErrorCode.Should().Be("401");
        }

        [Fact]
        public void TradingName_ShouldValidateSpecialCharactersAndLength()
        {
            var vacancy = new Vacancy() {
                EmployerName = "£$$%$%£$<>" + new string('a', 100),
                EmployerNameOption = EmployerNameOption.TradingName
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TradingName);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(2);
            result.Errors.Any(e => e.ErrorCode == "402").Should().BeTrue();
            result.Errors.Any(e => e.ErrorCode == "403").Should().BeTrue();
        }

        [Fact]
        public void Anonymous_ShouldValidateEmpty()        
        {
            var vacancy = new Vacancy()
            {
                EmployerName = string.Empty,
                EmployerNameOption = EmployerNameOption.Anonymous,
                AnonymousReason = "a valid reason"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerName));
            result.Errors[0].ErrorCode.Should().Be("405");
        }

        [Fact]
        public void Anonymous_ShouldValidateSpecialCharactersAndLength()
        {
            var vacancy = new Vacancy() {
                EmployerName = "£$$%$%£$<>" + new string('a', 100),
                EmployerNameOption = EmployerNameOption.Anonymous,
                AnonymousReason = "a valid reason"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(2);
            result.Errors.Any(e => e.ErrorCode == "406").Should().BeTrue();
            result.Errors.Any(e => e.ErrorCode == "407").Should().BeTrue();
        }

        [Theory]
        [InlineData("some text bother")]
        [InlineData("some text dang")]
        [InlineData("some text drat")]
        [InlineData("some text balderdash")]
        public void Anonymous_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
        {
            var vacancy = new Vacancy()
            {
                EmployerName = freeText,
                EmployerNameOption = EmployerNameOption.Anonymous,
                AnonymousReason = "a valid reason"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);
            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorCode.Should().Be("603");
        }

        [Theory]
        [InlineData("some textbother")]
        [InlineData("some textdang")]
        [InlineData("some textdrat")]
        [InlineData("some textbalderdash")]
        public void Anonymous_Should_Not_FailIfContainsWordsFromTheProfanityList(string freeText)
        {
            var vacancy = new Vacancy()
            {
                EmployerName = freeText,
                EmployerNameOption = EmployerNameOption.Anonymous,
                AnonymousReason = "a valid reason"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);
            result.HasErrors.Should().BeFalse();
        }


        [Fact]
        public void ErrorIfEmployerNameDoesNotExist()
        {
            const long ukprn = 12345678;
            const string employerAccountId = "employer-account-id";
            const string accountLegalEntityPublicHashedId = "1234";

            var vacancy = new Vacancy
            {
                OwnerType = OwnerType.Provider,
                TrainingProvider = new TrainingProvider { Ukprn = ukprn },
                EmployerAccountId = employerAccountId,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
                EmployerName = "Some Employer Name"
            };

            MockTrainingProviderSummaryProvider.Setup(p => p.GetAsync(ukprn)).ReturnsAsync(new TrainingProviderSummary { IsTrainingProviderMainOrEmployerProfile = true });
            MockProviderRelationshipsService.Setup(p => p.HasProviderGotEmployersPermissionAsync(ukprn, employerAccountId, accountLegalEntityPublicHashedId, OperationType.Recruitment))
                .ReturnsAsync(true);
            MockProviderRelationshipsService.Setup(p => p.GetLegalEntitiesForProviderAsync(ukprn, OperationType.Recruitment))
                .ReturnsAsync(new List<EmployerInfo>
                {
                    new()
                    {
                        Name = "some name",
                        EmployerAccountId = employerAccountId,
                        LegalEntities =
                        [
                            new LegalEntity
                            {
                                AccountLegalEntityPublicHashedId = "different-id"
                            }
                        ]
                    }
                });

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerName);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(Vacancy.EmployerName));
            result.Errors[0].ErrorCode.Should().Be(ErrorCodes.EmployerNameMustBeValid);
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerName);
        }

        [Fact]
        public void NoErrorIfEmployerDoExist()
        {
            const long ukprn = 12345678;
            const string employerAccountId = "employer-account-id";
            const string accountLegalEntityPublicHashedId = "1234";

            var vacancy = new Vacancy
            {
                OwnerType = OwnerType.Provider,
                TrainingProvider = new TrainingProvider { Ukprn = ukprn },
                EmployerAccountId = employerAccountId,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
                EmployerName = "Some Employer Name"
            };

            MockTrainingProviderSummaryProvider.Setup(p => p.GetAsync(ukprn)).ReturnsAsync(new TrainingProviderSummary { IsTrainingProviderMainOrEmployerProfile = true });
            MockProviderRelationshipsService.Setup(p => p.HasProviderGotEmployersPermissionAsync(ukprn, employerAccountId, accountLegalEntityPublicHashedId, OperationType.Recruitment))
                .ReturnsAsync(true);
            MockProviderRelationshipsService.Setup(p => p.GetLegalEntitiesForProviderAsync(ukprn, OperationType.Recruitment))
                .ReturnsAsync(new List<EmployerInfo>
                {
                    new()
                    {
                        Name = "some name",
                        EmployerAccountId = employerAccountId,
                        LegalEntities =
                        [
                            new LegalEntity
                            {
                                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId
                            }
                        ]
                    }
                });

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerName);

            result.HasErrors.Should().BeFalse();
        }
    }
}