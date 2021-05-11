using Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class TrainingProviderUkprnValidationTests : VacancyValidationTestsBase
    {
        [Fact]
        public void NoErrorsWhenTrainingProviderUkprnIsValid()
        {
            var vacancy = new Vacancy
            {
                TrainingProvider = new TrainingProvider { Ukprn = 12345678 }
            };

            MockTrainingProviderSummaryProvider.Setup(p => p.GetAsync(12345678)).ReturnsAsync(new TrainingProviderSummary());

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProvider);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void ErrorIfTrainingProviderIsNotInRoatp()
        {
            var vacancy = new Vacancy
            {
                TrainingProvider = new TrainingProvider { Ukprn = 12345678 }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProvider);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingProvider));
            result.Errors[0].ErrorCode.Should().Be(ErrorCodes.TrainingProviderMustExist);
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingProvider);
        }

        [Fact]
        public void ErrorIfTrainingProviderIsBlocked()
        {
            var vacancy = new Vacancy
            {
                TrainingProvider = new TrainingProvider { Ukprn = 12345678 }
            };

            MockTrainingProviderSummaryProvider.Setup(p => p.GetAsync(12345678)).ReturnsAsync(new TrainingProviderSummary());

            MockBlockedOrganisationRepo.Setup(b => b.GetByOrganisationIdAsync("12345678"))
                .ReturnsAsync(new BlockedOrganisation {BlockedStatus = BlockedStatus.Blocked});

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProvider);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingProvider));
            result.Errors[0].ErrorCode.Should().Be(ErrorCodes.TrainingProviderMustNotBeBlocked);
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingProvider);
        }

        [Fact]
        public void ErrorIfTrainingProviderIsNull()
        {
            var vacancy = new Vacancy
            {
                TrainingProvider = null
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProvider);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingProvider));
            result.Errors[0].ErrorCode.Should().Be("101");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingProvider);
        }

        [Fact]
        public void EmptyUkprnNotAllowed()
        {
            var vacancy = new Vacancy
            {
                TrainingProvider = new TrainingProvider { Ukprn = null }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProvider);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingProvider));
            result.Errors[0].ErrorCode.Should().Be("101");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingProvider);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1234)]
        [InlineData(123456789)]
        public void TrainingProviderUkprnMustBe8Digits(long? ukprn)
        {
            var vacancy = new Vacancy
            {
                TrainingProvider = new TrainingProvider { Ukprn = ukprn }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProvider);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingProvider));
            result.Errors[0].ErrorCode.Should().Be("99");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingProvider);
        }

        [Fact]
        public void ErrorIfProviderVacancyDoesNotHaveEmployerPermission()
        {
            const long ukprn = 12345678;
            const string employerAccountId = "employer-account-id";
            const string accountLegalEntityPublicHashedId = "1234";

            var vacancy = new Vacancy
            {
                OwnerType = OwnerType.Provider,
                TrainingProvider = new TrainingProvider { Ukprn = ukprn },
                EmployerAccountId = employerAccountId,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId
            };

            MockTrainingProviderSummaryProvider.Setup(p => p.GetAsync(ukprn)).ReturnsAsync(new TrainingProviderSummary());

            MockProviderRelationshipsService.Setup(p => p.HasProviderGotEmployersPermissionAsync(ukprn, employerAccountId, accountLegalEntityPublicHashedId, OperationType.Recruitment))
                .ReturnsAsync(false);

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProvider);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(string.Empty);
            result.Errors[0].ErrorCode.Should().Be(ErrorCodes.TrainingProviderMustHaveEmployerPermission);
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingProvider);
        }

        [Fact]
        public void ShouldNotErrorIfProviderVacancyHasEmployerPermission()
        {
            const long ukprn = 12345678;
            const string employerAccountId = "employer-account-id";
            const string accountLegalEntityPublicHashedId = "1234";

            var vacancy = new Vacancy
            {
                OwnerType = OwnerType.Provider,
                TrainingProvider = new TrainingProvider { Ukprn = ukprn },
                EmployerAccountId = employerAccountId,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId
            };

            MockTrainingProviderSummaryProvider.Setup(p => p.GetAsync(ukprn)).ReturnsAsync(new TrainingProviderSummary());

            MockProviderRelationshipsService.Setup(p => p.HasProviderGotEmployersPermissionAsync(ukprn, employerAccountId, accountLegalEntityPublicHashedId, OperationType.Recruitment))
                .ReturnsAsync(true);

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProvider);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }
    }
}