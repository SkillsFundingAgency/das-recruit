using Esfa.Recruit.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

[TestFixture]
public class EmployerLocationValidationTests : VacancyValidationTestsBase
{
    [Test]
    public void NoErrorsWhenLocationIsValid()
    {
        var vacancy = new Vacancy
        {
            EmployerLocations = [new Address
            {
                AddressLine1 = "address line 1",
                AddressLine2 = "address line 2",
                AddressLine3 = "address line 3",
                Postcode = "SW1 1AB",
                Country = Constants.EnglandCountryCode
            }],
            EmployerLocationOption = AvailableWhere.OneLocation
        };

        MockLocationsService.Setup(p => p.IsPostcodeInEnglandAsync("SW1 1AB")).ReturnsAsync(true);

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Test]
    public void ErrorWhenLocationIsOutOfArea()
    {
        var vacancy = new Vacancy
        {
            EmployerLocations = [new Address
            {
                AddressLine1 = "address line 1",
                AddressLine2 = "address line 2",
                AddressLine3 = "address line 3",
                Postcode = "CF10 3RB",
                Country = "Wales"
            }],
            EmployerLocationOption = AvailableWhere.OneLocation
        };

        MockLocationsService.Setup(p => p.IsPostcodeInEnglandAsync("CF10 3RB")).ReturnsAsync(false);

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }
}