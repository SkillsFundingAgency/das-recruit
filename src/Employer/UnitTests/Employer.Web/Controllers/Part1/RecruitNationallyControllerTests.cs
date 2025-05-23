﻿using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers.Part1;
using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.RecruitNationally;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers.Part1;

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
        var vacancyRouteModel = new VacancyRouteModel { VacancyId = vacancy.Id, EmployerAccountId = vacancy.EmployerAccountId, };
        utility
            .Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.RecruitNationally_Get))
            .ReturnsAsync(vacancy);
        
        // act
        var result = (await _sut.RecruitNationally(utility.Object, Mock.Of<IReviewSummaryService>(), vacancyRouteModel) as ViewResult)?.Model as RecruitNationallyViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(vacancy.EmployerAccountId);
        result.AdditionalInformation.Should().Be(vacancy.EmployerLocationInformation);
    }
    
    [Test, MoqAutoData]
    public async Task When_Getting_RecruitNationally_For_Referred_Vacancy_Then_The_Review_Is_Set(
        [Frozen] Vacancy vacancy,
        Mock<IUtility> utility,
        Mock<IReviewSummaryService> reviewSummaryService)
    {
        // arrange
        vacancy.Status = VacancyStatus.Referred;
        var vacancyRouteModel = new VacancyRouteModel { VacancyId = vacancy.Id, EmployerAccountId = vacancy.EmployerAccountId, };
        utility
            .Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.RecruitNationally_Get))
            .ReturnsAsync(vacancy);
        
        // act
        var result = (await _sut.RecruitNationally(utility.Object, reviewSummaryService.Object, vacancyRouteModel) as ViewResult)?.Model as RecruitNationallyViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.Review.Should().NotBeNull();
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_Invalid_Model_To_RecruitNationally_Then_ViewModel_Is_Returned([Frozen] Vacancy vacancy, Mock<IUtility> utility)
    {
        // arrange
        var model = new RecruitNationallyEditModel
        {
            AdditionalInformation = null,
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId
        };
        utility
            .Setup(x => x.GetAuthorisedVacancyForEditAsync(model, RouteNames.RecruitNationally_Post))
            .ReturnsAsync(vacancy);
        _vacancyLocationService
            .Setup(x => x.UpdateDraftVacancyLocations(vacancy, It.IsAny<VacancyUser>(), AvailableWhere.AcrossEngland, null, model.AdditionalInformation))
            .ReturnsAsync(new UpdateVacancyLocationsResult(new EntityValidationResult { Errors = new List<EntityValidationError> { new (1, "propertyName", "errorMessage", "") } }));
        
        // act
        var result = (await _sut.RecruitNationally(_vacancyLocationService.Object, utility.Object, Mock.Of<IReviewSummaryService>(), model) as ViewResult)?.Model as RecruitNationallyViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(vacancy.EmployerAccountId);
        result.AdditionalInformation.Should().BeNull();
    }
    
    [Test]
    [MoqInlineAutoData(true, RouteNames.EmployerTaskListGet)]
    [MoqInlineAutoData(false, RouteNames.EmployerCheckYourAnswersGet)]
    public async Task When_Posting_Valid_Model_To_RecruitNationally_Then_RedirectToRoute_Is_Return(bool isWizard, string expectedRouteName, [Frozen] Vacancy vacancy, Mock<IUtility> utility)
    {
        // arrange
        var model = new RecruitNationallyEditModel
        {
            AdditionalInformation = null,
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId
        };
        utility
            .Setup(x => x.GetAuthorisedVacancyForEditAsync(model, RouteNames.RecruitNationally_Post))
            .ReturnsAsync(vacancy);
        _vacancyLocationService
            .Setup(x => x.UpdateDraftVacancyLocations(vacancy, It.IsAny<VacancyUser>(), AvailableWhere.AcrossEngland, null, model.AdditionalInformation))
            .ReturnsAsync(new UpdateVacancyLocationsResult(null));
        
        // act
        var result = await _sut.RecruitNationally(_vacancyLocationService.Object, utility.Object, Mock.Of<IReviewSummaryService>(), model, isWizard) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(expectedRouteName);
    }
}