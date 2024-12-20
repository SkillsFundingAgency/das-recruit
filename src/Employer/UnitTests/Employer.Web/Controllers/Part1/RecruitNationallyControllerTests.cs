using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers.Part1;
using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.RecruitNationally;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers.Part1;

public class RecruitNationallyControllerTests
{
    [Test, MoqAutoData]
    public async Task When_Getting_RecruitNationally_Then_ViewModel_Is_Returned(
        [Frozen] Vacancy vacancy,
        Mock<IUtility> utility,
        [Greedy] RecruitNationallyController sut)
    {
        // arrange
        var vacancyRouteModel = new VacancyRouteModel { VacancyId = vacancy.Id, EmployerAccountId = vacancy.EmployerAccountId, };
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.IsAny<VacancyRouteModel>(), It.IsAny<string>())).ReturnsAsync(vacancy);
        
        // act
        var result = (await sut.RecruitNationally(utility.Object, vacancyRouteModel) as ViewResult)?.Model as RecruitNationallyViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(vacancy.EmployerAccountId);
        result.AdditionalInformation.Should().Be(vacancy.EmployerLocationInformation);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_Invalid_Model_To_RecruitNationally_Then_ViewModel_Is_Returned(
        [Frozen] Vacancy vacancy,
        Mock<IUtility> utility,
        IRecruitVacancyClient recruitVacancyClient,
        [Greedy] RecruitNationallyController sut)
    {
        // arrange
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.IsAny<VacancyRouteModel>(), It.IsAny<string>())).ReturnsAsync(vacancy);
        var model = new RecruitNationallyEditModel
        {
            AdditionalInformation = null,
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId
        };
        
        // act
        var result = (await sut.RecruitNationally(recruitVacancyClient, utility.Object, model) as ViewResult)?.Model as RecruitNationallyViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(vacancy.EmployerAccountId);
        result.AdditionalInformation.Should().Be(vacancy.EmployerLocationInformation);
    }
    
    [Test]
    [MoqInlineAutoData(true, RouteNames.EmployerTaskListGet)]
    [MoqInlineAutoData(false, RouteNames.EmployerCheckYourAnswersGet)]
    public async Task When_Posting_Valid_Model_To_RecruitNationally_Then_RedirectToRoute_Is_Return(
        bool isWizard,
        string expectedRouteName,
        [Frozen] Vacancy vacancy,
        Mock<IUtility> utility,
        Mock<IRecruitVacancyClient> recruitVacancyClient,
        [Greedy] RecruitNationallyController sut)
    {
        // arrange
        sut.AddControllerContext().WithUser(Guid.NewGuid());
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.IsAny<VacancyRouteModel>(), It.IsAny<string>())).ReturnsAsync(vacancy);
        var model = new RecruitNationallyEditModel
        {
            AdditionalInformation = null,
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId
        };

        recruitVacancyClient
            .Setup(x => x.Validate(It.IsAny<Vacancy>(), It.IsAny<VacancyRuleSet>()))
            .Returns(new EntityValidationResult());
        
        // act
        var result = await sut.RecruitNationally(recruitVacancyClient.Object, utility.Object, model, isWizard) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(expectedRouteName);
        recruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(vacancy, It.IsAny<VacancyUser>()));
    }
}