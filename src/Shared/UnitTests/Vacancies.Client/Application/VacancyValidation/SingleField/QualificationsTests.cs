using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class QualificationsTests : VacancyValidationTestsBase
    {
        public QualificationsTests()
        {
            MockQualificationsProvider.Setup(q => q.GetQualificationsAsync()).ReturnsAsync(new List<string>{"type"});
        }

        [Fact]
        public void NoErrorWhenQualificationsAreValid()
        {
            var vacancy = GetVacancy();
            var result = Validator.Validate(vacancy, VacancyRuleSet.Qualifications);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }
        
        [Fact]
        public void HasNoErrorWhenOptedNotToAddQualifications()
        {
            var vacancy = new Vacancy
            {
                Qualifications = null,
                HasOptedToAddQualifications = false
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Qualifications);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }
        
        public static IEnumerable<object[]> NullOrZeroQualificationCollection =>
            new List<object[]>
            {
                new object[] {null},
                new object[] {new List<Qualification>()},
            };

        [Theory]
        [MemberData(nameof(NullOrZeroQualificationCollection))]
        public void QualificationsMustNotBeNullOrZeroCount(List<Qualification> qualifications)
        {
            var vacancy = new Vacancy
            {
                Qualifications = qualifications,
                HasOptedToAddQualifications = true
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Qualifications);

            AssertSingleError(result, nameof(vacancy.Qualifications), "52");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void QualificationTypeMustNotBeEmpty(string emptyString)
        {
            var vacancy = GetVacancy(qualificationType: emptyString);

            var result = Validator.Validate(vacancy, VacancyRuleSet.Qualifications);

            var propertyName = $"{nameof(vacancy.Qualifications)}[0].{nameof(Qualification.QualificationType)}";
            AssertSingleError(result, propertyName, "53");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void SubjectMustNotBeEmpty(string emptyString)
        {
            var vacancy = GetVacancy(subject: emptyString);

            var result = Validator.Validate(vacancy, VacancyRuleSet.Qualifications);

            var propertyName = $"{nameof(vacancy.Qualifications)}[0].{nameof(Qualification.Subject)}";
            AssertSingleError(result, propertyName, "54");
        }

        [Fact]
        public void SubjectMustNotBeLongerThanMaxLength()
        {
            var vacancy = GetVacancy(subject: new string('a', 51));

            var result = Validator.Validate(vacancy, VacancyRuleSet.Qualifications);

            var propertyName = $"{nameof(vacancy.Qualifications)}[0].{nameof(Qualification.Subject)}";
            AssertSingleError(result, propertyName, "7");
        }

        [Fact]
        public void SubjectMustNotContainInvalidCharacters()
        {
            var vacancy = GetVacancy(subject: "<");

            var result = Validator.Validate(vacancy, VacancyRuleSet.Qualifications);

            var propertyName = $"{nameof(vacancy.Qualifications)}[0].{nameof(Qualification.Subject)}";
            AssertSingleError(result, propertyName, "6");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GradeMustNotBeEmpty(string emptyString)
        {
            var vacancy = GetVacancy(grade: emptyString);

            var result = Validator.Validate(vacancy, VacancyRuleSet.Qualifications);

            var propertyName = $"{nameof(vacancy.Qualifications)}[0].{nameof(Qualification.Grade)}";
            AssertSingleError(result, propertyName, "55");
        }

        [Fact]
        public void GradeMustNotBeLongerThanMaxLength()
        {
            var vacancy = GetVacancy(grade: new string('a', 31));

            var result = Validator.Validate(vacancy, VacancyRuleSet.Qualifications);

            var propertyName = $"{nameof(vacancy.Qualifications)}[0].{nameof(Qualification.Grade)}";
            AssertSingleError(result, propertyName, "7");
        }

        [Fact]
        public void GradeMustNotContainInvalidCharacters()
        {
            var vacancy = GetVacancy(grade: "<");
            
            var result = Validator.Validate(vacancy, VacancyRuleSet.Qualifications);

            var propertyName = $"{nameof(vacancy.Qualifications)}[0].{nameof(Qualification.Grade)}";
            AssertSingleError(result, propertyName, "6");
        }

        [Fact]
        public void WeightingMustNotBeNull()
        {
            var vacancy = GetVacancy(weighting: null);

            var result = Validator.Validate(vacancy, VacancyRuleSet.Qualifications);

            var propertyName = $"{nameof(vacancy.Qualifications)}[0].{nameof(Qualification.Weighting)}";
            AssertSingleError(result, propertyName, "56");
        }

        private void AssertSingleError(EntityValidationResult result, string propertyName, string errorCode)
        {
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(propertyName);
            result.Errors[0].ErrorCode.Should().Be(errorCode);
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Qualifications);
        }

        private Vacancy GetVacancy(string qualificationType = "type", string subject = "subject",
            string grade = "grade", QualificationWeighting? weighting = QualificationWeighting.Desired)
        {
            var qualification = new Qualification
            {
                QualificationType = qualificationType,
                Subject = subject,
                Grade = grade,
                Weighting = weighting
            };

            return new Vacancy
            {
                Qualifications = new List<Qualification> { qualification },
                HasOptedToAddQualifications = true
            };
        }
        
    }
}
