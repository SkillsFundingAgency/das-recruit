using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1
{
    public class TraineeSectorOrchestratorTests
    {
        [Test, MoqAutoData]
        public async Task When_Calling_Get_Then_Returns_Routes_In_The_View_Model(
            Vacancy vacancy,
            VacancyRouteModel vacancyRouteModel,
            List<IApprenticeshipRoute> routes,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            TraineeSectorOrchestrator orchestrator)
        {
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.TraineeSector_Get))
                .ReturnsAsync(vacancy);
            recruitVacancyClient.Setup(x => x.GetApprenticeshipRoutes()).ReturnsAsync(routes);
            providerVacancyClient.Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, vacancy.EmployerAccountId)).ReturnsAsync(
                new EmployerInfo
                {
                    LegalEntities = new List<LegalEntity>
                    {
                        new LegalEntity()
                    }
                });
            
            var actual = await orchestrator.GetTraineeSectorViewModelAsync(vacancyRouteModel);

            actual.Routes.Should().BeEquivalentTo(routes.Select(c => new ApprenticeshipRouteViewModel
            {
                Id = c.Id,
                Name = c.Route
            }));
            actual.Title.Should().Be(vacancy.Title);
            actual.VacancyId.Should().Be(vacancy.Id);
            actual.Ukprn.Should().Be(vacancyRouteModel.Ukprn);
            actual.HasMoreThanOneLegalEntity.Should().BeFalse();
        }
        [Test, MoqAutoData]
        public async Task When_Calling_Get_Then_Returns_Routes_In_The_View_Model_And_Set_Legal_Entity_Count_True_If_More_Than_One(
            Vacancy vacancy,
            VacancyRouteModel vacancyRouteModel,
            EmployerInfo employerInfo,
            List<IApprenticeshipRoute> routes,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient)
        {
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.TraineeSector_Get))
                .ReturnsAsync(vacancy);
            recruitVacancyClient.Setup(x => x.GetApprenticeshipRoutes()).ReturnsAsync(routes);
            providerVacancyClient.Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, vacancy.EmployerAccountId)).ReturnsAsync(employerInfo);
            var orchestrator = new TraineeSectorOrchestrator(Mock.Of<ILogger<TraineeSectorOrchestrator>>(),
                recruitVacancyClient.Object, providerVacancyClient.Object, utility.Object, Mock.Of<IReviewSummaryService>(), Mock.Of<IProviderRelationshipsService>(), new ServiceParameters("Apprenticeship"));
            
            var actual = await orchestrator.GetTraineeSectorViewModelAsync(vacancyRouteModel);

            actual.Routes.Should().BeEquivalentTo(routes.Select(c => new ApprenticeshipRouteViewModel
            {
                Id = c.Id,
                Name = c.Route
            }));
            actual.Title.Should().Be(vacancy.Title);
            actual.VacancyId.Should().Be(vacancy.Id);
            actual.Ukprn.Should().Be(vacancyRouteModel.Ukprn);
            actual.HasMoreThanOneLegalEntity.Should().BeTrue();
        }

        [Test, MoqAutoData]
        public async Task Then_If_Traineeship_Then_Employers_Are_Filtered_By_Permissions(
            Vacancy vacancy,
            VacancyRouteModel vacancyRouteModel,
            EmployerInfo employerInfo,
            List<IApprenticeshipRoute> routes,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            [Frozen] Mock<IReviewSummaryService> reviewSummaryService)
        {
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.TraineeSector_Get))
                .ReturnsAsync(vacancy);
            recruitVacancyClient.Setup(x => x.GetApprenticeshipRoutes()).ReturnsAsync(routes);
            providerVacancyClient.Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, vacancy.EmployerAccountId)).ReturnsAsync(employerInfo);
            providerRelationshipsService
                .Setup(x => x.GetAccountLegalEntitiesForProvider(vacancyRouteModel.Ukprn, vacancy.EmployerAccountId,
                    OperationType.RecruitmentRequiresReview)).ReturnsAsync(employerInfo.LegalEntities.Take(employerInfo.LegalEntities.Count-1).Select(c=>new LegalEntity
                {
                   AccountLegalEntityPublicHashedId = c.AccountLegalEntityPublicHashedId
                })) ;
            var orchestrator = new TraineeSectorOrchestrator(Mock.Of<ILogger<TraineeSectorOrchestrator>>(),
                recruitVacancyClient.Object, providerVacancyClient.Object, utility.Object, reviewSummaryService.Object, providerRelationshipsService.Object, new ServiceParameters("Traineeship"));
            
            var actual = await orchestrator.GetTraineeSectorViewModelAsync(vacancyRouteModel);

            actual.HasMoreThanOneLegalEntity.Should().BeFalse();
        }
        
        [Test, MoqAutoData]
        public async Task When_Calling_Post_Then_Updates_VacancyClient(
            Vacancy vacancy,
            VacancyUser vacancyUser,
            TraineeSectorEditModel editModel,
            EntityValidationResult validationResult,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            TraineeSectorOrchestrator orchestrator)
        {
            validationResult.Errors = new List<EntityValidationError>();
            utility
                .Setup(x => x.GetAuthorisedVacancyForEditAsync(editModel, RouteNames.TraineeSector_Post))
                .ReturnsAsync(vacancy);
            recruitVacancyClient
                .Setup(x => x.Validate(vacancy, VacancyRuleSet.RouteId))
                .Returns(validationResult);

            var actual = await orchestrator.PostTraineeSectorEditModelAsync(editModel, vacancyUser);

            actual.Success.Should().BeTrue();
            recruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(vacancy, vacancyUser), Times.Once);
            vacancy.ProviderReviewFieldIndicators
                .FirstOrDefault(indicator => indicator.FieldIdentifier == FieldIdentifiers.TraineeSector).Should().BeNull();
        }

        [Test, MoqAutoData]
        public async Task When_Calling_Post_And_Review_Rejected_Then_Updates_VacancyClient_And_FieldIndicators(
            Vacancy vacancy,
            VacancyUser vacancyUser,
            TraineeSectorEditModel editModel,
            EntityValidationResult validationResult,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            TraineeSectorOrchestrator orchestrator)
        {
            validationResult.Errors = new List<EntityValidationError>();
            vacancy.Status = VacancyStatus.Rejected;
            utility
                .Setup(x => x.GetAuthorisedVacancyForEditAsync(editModel, RouteNames.TraineeSector_Post))
                .ReturnsAsync(vacancy);
            recruitVacancyClient
                .Setup(x => x.Validate(vacancy, VacancyRuleSet.RouteId))
                .Returns(validationResult);

            var actual = await orchestrator.PostTraineeSectorEditModelAsync(editModel, vacancyUser);

            actual.Success.Should().BeTrue();
            recruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(vacancy, vacancyUser), Times.Once);
            vacancy.ProviderReviewFieldIndicators
                .FirstOrDefault(indicator => indicator.FieldIdentifier == FieldIdentifiers.TraineeSector).Should().NotBeNull()
                .And.Match<ProviderReviewFieldIndicator>(indicator => indicator.IsChangeRequested);
        }
        
        [Test, MoqAutoData]
        public async Task When_Calling_Post_And_Invalid_Then_Not_Update_VacancyClient(
            Vacancy vacancy,
            VacancyUser vacancyUser,
            TraineeSectorEditModel editModel,
            EntityValidationResult validationResult,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            TraineeSectorOrchestrator orchestrator)
        {
            utility
                .Setup(x => x.GetAuthorisedVacancyForEditAsync(editModel, RouteNames.TraineeSector_Post))
                .ReturnsAsync(vacancy);
            recruitVacancyClient
                .Setup(x => x.Validate(vacancy, VacancyRuleSet.RouteId))
                .Returns(validationResult);

            var actual = await orchestrator.PostTraineeSectorEditModelAsync(editModel, vacancyUser);

            actual.Success.Should().BeFalse();
            recruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(vacancy, vacancyUser), Times.Never);
        }

    }
}