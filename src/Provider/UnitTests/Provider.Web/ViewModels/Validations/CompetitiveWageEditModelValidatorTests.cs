using Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Provider.Web.ViewModels.Validations.Fluent;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.ViewModels.Validations;
public class CompetitiveWageEditModelValidatorTests
{
    private CompetitiveWageEditModelValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new CompetitiveWageEditModelValidator();
    }

    [Test]
    public void IsSalaryAboveNationalMinimumWage_ShouldHaveError_WhenIsSalaryAboveNationalMinimumWageIsNull()
    {
        var competitiveWageEditModel = new CompetitiveWageEditModel { IsSalaryAboveNationalMinimumWage = null };

        var result = _validator.TestValidate(competitiveWageEditModel);

        result.ShouldHaveValidationErrorFor(x => x.IsSalaryAboveNationalMinimumWage)
                 .WithErrorMessage(CompetitiveWageEditModelValidator.IsSalaryAboveNationalMinimumWageRequired);
    }

    [Test]
    public void IsSalaryAboveNationalMinimumWage_ShouldNotHaveError_WhenIsSalaryAboveNationalMinimumWageIsNotNull()
    {
        var competitiveWageEditModel = new CompetitiveWageEditModel { IsSalaryAboveNationalMinimumWage = true };

        var result = _validator.TestValidate(competitiveWageEditModel);

        result.ShouldNotHaveValidationErrorFor(x => x.IsSalaryAboveNationalMinimumWage);
    }
}

