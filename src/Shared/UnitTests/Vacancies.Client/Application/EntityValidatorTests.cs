using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation;
using FluentValidation;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application;

public class EntityValidatorTests : VacancyValidationTestsBase
{
    [Fact]
    public void ValidateReturnsFailWhenValidationFails()
    {
        var validator = new EntityValidator<TestEntity, TestEntityRules>(new TestEntityFluentValidator());

        var testEntity = new TestEntity
        {
            TestProperty = 3
        };

        var result = validator.Validate(testEntity, TestEntityRules.All);

        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void ValidateReturnsListOfValidationErrorWhenValidationFails()
    {
        var validator = new EntityValidator<TestEntity, TestEntityRules>(new TestEntityFluentValidator());

        var testEntity = new TestEntity
        {
            TestProperty = 3
        };

        var result = validator.Validate(testEntity, TestEntityRules.All);

        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void ValidateReturnsNoErrorWhenValidationPasses()
    {
        var validator = new EntityValidator<TestEntity, TestEntityRules>(new TestEntityFluentValidator());

        var testEntity = new TestEntity
        {
            TestProperty = 6
        };

        var result = validator.Validate(testEntity, TestEntityRules.All);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }
}

public class TestEntity 
{
    public int TestProperty { get; set; }
}

public enum TestEntityRules
{
    All
}

public class TestEntityFluentValidator : AbstractValidator<TestEntity>
{
    public TestEntityFluentValidator()
    {
        RuleFor(x => x.TestProperty)
            .GreaterThan(5)
            .WithState(_ => (long)TestEntityRules.All);
    }
}