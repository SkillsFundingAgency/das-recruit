using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Validations;
public class WageEditModelValidatorTests
{
    private WageEditModelValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new WageEditModelValidator();
    }

    [Test]
    public void WageType_ShouldHaveError_WhenWageTypeIsNull()
    {
        var wageEditModel = new WageEditModel { WageType = null };

        var result = _validator.TestValidate(wageEditModel);

        result.ShouldHaveValidationErrorFor(x => x.WageType)
                 .WithErrorMessage(WageEditModelValidator.WageTypeRequired);
    }

    [Test]
    public void WageType_ShouldNotHaveError_WhenWageTypeIsNotNull()
    {
        var wageEditModel = new WageEditModel { WageType = WageType.FixedWage };

        var result = _validator.TestValidate(wageEditModel);

        result.ShouldNotHaveValidationErrorFor(x => x.WageType);
    }
}

