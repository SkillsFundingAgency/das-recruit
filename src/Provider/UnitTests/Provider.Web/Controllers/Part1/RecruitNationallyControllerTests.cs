using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers.Part1;
using Esfa.Recruit.Provider.Web.Models.AddLocation;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.RecruitNationally;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers.Part1;

public class RecruitNationallyControllerTests
{
    private RecruitNationallyController _sut;
    private Mock<IVacancyLocationService> _vacancyLocationService;

    [SetUp]
    public void SetUp()
    {
        _sut = new RecruitNationallyController();
        _sut.AddControllerContext().WithUser(Guid.NewGuid());
        _vacancyLocationService = new Mock<IVacancyLocationService>();
    }


    [Test, MoqAutoData]
    public async Task When_Getting_RecruitNationally_Then_ViewModel_Is_Returned([Frozen] Vacancy vacancy, Mock<IUtility> utility)
    {
        // arrange
        var vacancyRouteModel = new VacancyRouteModel { VacancyId = vacancy.Id, Ukprn = new Random().Next(), };
        utility
            .Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.RecruitNationally_Get))
            .ReturnsAsync(vacancy);
        
        // act
        var result = (await _sut.RecruitNationally(utility.Object, vacancyRouteModel) as ViewResult)?.Model as RecruitNationallyViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.Ukprn.Should().Be(vacancyRouteModel.Ukprn);
        result.AdditionalInformation.Should().Be(vacancy.EmployerLocationInformation);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_Invalid_Model_To_RecruitNationally_Then_ViewModel_Is_Returned([Frozen] Vacancy vacancy, Mock<IUtility> utility)
    {
        // arrange
        var model = new RecruitNationallyEditModel
        {
            AdditionalInformation = null,
            VacancyId = vacancy.Id,
            Ukprn = new Random().Next()
        };
        
        _sut
            .AddControllerContext()
            .WithUser(Guid.NewGuid())
            .WithClaim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier, model.Ukprn.ToString());
        utility
            .Setup(x => x.GetAuthorisedVacancyForEditAsync(model, RouteNames.RecruitNationally_Post))
            .ReturnsAsync(vacancy);
        _vacancyLocationService
            .Setup(x => x.UpdateDraftVacancyLocations(vacancy, It.IsAny<VacancyUser>(), AvailableWhere.AcrossEngland, null, model.AdditionalInformation))
            .ReturnsAsync(new UpdateVacancyLocationsResult(new EntityValidationResult { Errors = new List<EntityValidationError> { new (1, "propertyName", "errorMessage", "") } }));
        
        // act
        var result = (await _sut.RecruitNationally(_vacancyLocationService.Object, utility.Object, model) as ViewResult)?.Model as RecruitNationallyViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.Ukprn.Should().Be(model.Ukprn);
        result.AdditionalInformation.Should().BeNull();
    }
    
    [Test]
    [MoqInlineAutoData(false, RouteNames.ProviderTaskListGet)]
    [MoqInlineAutoData(true, RouteNames.ProviderCheckYourAnswersGet)]
    public async Task When_Posting_Valid_Model_To_RecruitNationally_Then_RedirectToRoute_Is_Return(
        bool isComplete,
        string expectedRouteName,
        [Frozen] Vacancy vacancy,
        [Frozen] Mock<IUtility> utility)
    {
        // arrange
        var model = new RecruitNationallyEditModel
        {
            AdditionalInformation = null,
            VacancyId = vacancy.Id,
            Ukprn = new Random().Next()
        };

        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, RouteNames.RecruitNationally_Post)).ReturnsAsync(vacancy);
        utility.Setup(x => x.IsTaskListCompleted(vacancy)).Returns(isComplete);
        
        _sut
            .AddControllerContext()
            .WithUser(Guid.NewGuid())
            .WithClaim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier, model.Ukprn.ToString());
        
        _vacancyLocationService
            .Setup(x => x.UpdateDraftVacancyLocations(vacancy, It.IsAny<VacancyUser>(), AvailableWhere.AcrossEngland, null, model.AdditionalInformation))
            .ReturnsAsync(new UpdateVacancyLocationsResult(null));
        
        // act
        var result = await _sut.RecruitNationally(_vacancyLocationService.Object, utility.Object, model) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(expectedRouteName);
    }
}