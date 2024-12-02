using System.Threading;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers.Part1;
using Esfa.Recruit.Employer.Web.Domain;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.AddLocation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers.Part1;

public class AddLocationControllerTests
{
    [Test, MoqAutoData]
    public async Task When_Getting_AddLocation_Then_ViewModel_Is_Returned(
        [Frozen] Vacancy vacancy,
        [Greedy] AddLocationController sut)
    {
        // arrange
        var vacancyRouteModel = new VacancyRouteModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
        };
        
        // act
        var result = (await sut.AddLocation(vacancyRouteModel, MultipleLocationsJourneyOrigin.Many, true) as ViewResult)?.Model as AddLocationViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(vacancy.EmployerAccountId);
        result.Origin.Should().Be(MultipleLocationsJourneyOrigin.Many);
        result.Postcode.Should().Be(null);
    }

    [Test, MoqAutoData]
    public async Task When_Post_Is_Valid_Then_Redirect_To_Select_An_Address(
        AddLocationEditModel addLocationModel,
        [Frozen] Mock<IValidator<AddLocationEditModel>> validator,
        [Greedy] AddLocationController sut)
    {
        // arrange
        validator.Setup(x => x.ValidateAsync(It.IsAny<AddLocationEditModel>(), CancellationToken.None)).ReturnsAsync(new ValidationResult());
        
        // act
        var result = await sut.AddLocation(validator.Object, addLocationModel, MultipleLocationsJourneyOrigin.Many, true) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.SelectAnAddress_Get);
    }
    
    [Test, MoqAutoData]
    public async Task When_Post_Is_InValid_Then_Return_View(
        AddLocationEditModel addLocationModel,
        [Frozen] Vacancy vacancy,
        [Frozen] Mock<IValidator<AddLocationEditModel>> validator,
        [Greedy] AddLocationController sut)
    {
        // arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Postcode", "Is invalid"));
        validator.Setup(x => x.Validate(It.IsAny<AddLocationEditModel>())).Returns(validationResult);
        
        // act
        var result = (await sut.AddLocation(validator.Object, addLocationModel, MultipleLocationsJourneyOrigin.Many, true) as ViewResult)?.Model as AddLocationViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(addLocationModel.VacancyId);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(addLocationModel.EmployerAccountId);
        result.Origin.Should().Be(MultipleLocationsJourneyOrigin.Many);
        result.Postcode.Should().Be(addLocationModel.Postcode);
    }
}