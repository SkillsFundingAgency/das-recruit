using System.Threading;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers.Part1;
using Esfa.Recruit.Provider.Web.Models.AddLocation;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.AddLocation;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers.Part1;

public class AddLocationControllerTests
{
    [Test, MoqAutoData]
    public async Task When_Getting_AddLocation_Then_ViewModel_Is_Returned(
        [Frozen] Vacancy vacancy,
        [Greedy] AddLocationController sut)
    {
        // arrange
        var addLocationModel = new AddLocationJourneyModel()
        {
            VacancyId = vacancy.Id,
            Ukprn = new Random().Next(),
            Origin = MultipleLocationsJourneyOrigin.Many,
            Wizard = true
        };
        sut.AddControllerContext().WithTempData();
        
        // act
        var result = (await sut.AddLocation(addLocationModel) as ViewResult)?.Model as AddLocationViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.Ukprn.Should().Be(addLocationModel.Ukprn);
        result.Origin.Should().Be(MultipleLocationsJourneyOrigin.Many);
        result.Postcode.Should().Be(null);
    }

    [Test, MoqAutoData]
    public async Task When_Posting_To_AddLocation_With_Valid_Request_Then_Redirect_To_Select_An_Address(
        AddLocationEditModel addLocationModel,
        GetAddressesListResponse getAddressesListResponse,
        [Frozen] Mock<IGetAddressesClient> getAddressesClient,
        [Frozen] Mock<IValidator<AddLocationEditModel>> validator,
        [Greedy] AddLocationController sut)
    {
        // arrange
        validator.Setup(x => x.ValidateAsync(addLocationModel, CancellationToken.None)).ReturnsAsync(new ValidationResult());
        getAddressesClient.Setup(x => x.GetAddresses(It.IsAny<string>())).ReturnsAsync(getAddressesListResponse);
        sut.AddControllerContext().WithTempData();
        
        // act
        var result = await sut.AddLocation(validator.Object, getAddressesClient.Object, addLocationModel) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.SelectAnAddress_Get);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_To_AddLocation_With_Invalid_Request_Then_Return_View(
        AddLocationEditModel addLocationEditModel,
        [Frozen] Mock<IGetAddressesClient> getAddressesClient,
        [Frozen] Vacancy vacancy,
        [Frozen] Mock<IValidator<AddLocationEditModel>> validator,
        [Greedy] AddLocationController sut)
    {
        // arrange
        addLocationEditModel.Origin = MultipleLocationsJourneyOrigin.Many;
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Postcode", "Is invalid"));
        validator.Setup(x => x.Validate(addLocationEditModel)).Returns(validationResult);
        getAddressesClient.Setup(x => x.GetAddresses(It.IsAny<string>())).ReturnsAsync((GetAddressesListResponse)null);
        
        // act
        var result = (await sut.AddLocation(validator.Object, getAddressesClient.Object, addLocationEditModel) as ViewResult)?.Model as AddLocationViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(addLocationEditModel.VacancyId);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.Ukprn.Should().Be(addLocationEditModel.Ukprn);
        result.Origin.Should().Be(addLocationEditModel.Origin);
        result.Postcode.Should().Be(addLocationEditModel.Postcode);
    }
}