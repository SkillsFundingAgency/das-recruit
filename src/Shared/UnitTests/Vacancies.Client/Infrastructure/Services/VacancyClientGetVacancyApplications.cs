using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class VacancyClientGetVacancyApplications
    {
        [Test, MoqAutoData]
        public async Task Then_The_Service_Is_Called_And_VacancyApplications_Returned(
            long vacancyReference,
            List<ApplicationReview> applicationReviews,
            [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepository,
            VacancyClient vacancyClient)
        {
            //Arrange
            applicationReviewRepository.Setup(x => x.GetForVacancyAsync<ApplicationReview>(vacancyReference))
                .ReturnsAsync(applicationReviews);
            
            //Act
            var actual = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference);
            
            //Assert
            actual.Should().BeEquivalentTo(applicationReviews.Select(c=>(VacancyApplication)c).ToList());
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_Empty_List_If_No_Results(
            long vacancyReference,
            [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepository,
            VacancyClient vacancyClient)
        {
            //Arrange
            applicationReviewRepository.Setup(x => x.GetForVacancyAsync<ApplicationReview>(vacancyReference))
                .ReturnsAsync(new List<ApplicationReview>());
            
            //Act
            var actual = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference);
            
            //Assert
            actual.Should().BeEmpty();
        }
        
        [Test, MoqAutoData]
        public async Task Then_Returns_Empty_List_If_Null(
            long vacancyReference,
            [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepository,
            VacancyClient vacancyClient)
        {
            //Arrange
            applicationReviewRepository.Setup(x => x.GetForVacancyAsync<ApplicationReview>(vacancyReference))
                .ReturnsAsync((List<ApplicationReview>) null);
            
            //Act
            var actual = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference);
            
            //Assert
            actual.Should().BeEmpty();
        }

        [Test, MoqAutoData]
        public async Task GetVacancyApplicationsSortedAsync_ReturnsEmptyList_WhenNoApplications(
            long vacancyReference,
            [Frozen] Mock<IApplicationReviewRepository> mockAppReviewRepo,
            [Greedy] VacancyClient vacancyClient)
        {
            // Arrange
            mockAppReviewRepo.Setup(r => r.GetForVacancySortedAsync(vacancyReference, It.IsAny<SortColumn>(), It.IsAny<SortOrder>()))
                .ReturnsAsync([]);

            // Act
            var result = await vacancyClient.GetVacancyApplicationsSortedAsync(vacancyReference, SortColumn.DateApplied, SortOrder.Ascending, false);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test, MoqAutoData]
        public async Task GetVacancyApplicationsSortedAsync_DeserializesAddresses_AndSetsCandidateAppliedLocations(
            long vacancyReference,
            Address address,
            ApplicationReview applicationReview,
            [Frozen] Mock<IApplicationReviewRepository> mockAppReviewRepo,
            [Frozen] Mock<ITrainingProviderService> mockTrainingProviderService,
            [Greedy] VacancyClient vacancyClient)
        {
            // Arrange
            var applicationId = Guid.NewGuid();
            var addressJson = JsonConvert.SerializeObject(address);

            applicationReview.Application.ApplicationId = applicationId;
            applicationReview.Id = applicationId;
            applicationReview.IsWithdrawn = false;

            mockAppReviewRepo.Setup(r => r.GetForVacancySortedAsync(It.IsAny<long>(), It.IsAny<SortColumn>(), It.IsAny<SortOrder>()))
                .ReturnsAsync([applicationReview]);

            var applicationsResponse = new GetAllApplicationsResponse
            {
                Applications =
                [
                    new GetAllApplicationsResponse.Application
                    {
                        Id = applicationId,
                        EmploymentLocation = new GetAllApplicationsResponse.Location
                        {
                            Addresses =
                            [
                                new GetAllApplicationsResponse.Address
                                {
                                    AddressOrder = 1,
                                    FullAddress = addressJson,
                                    IsSelected = true
                                }
                            ]
                        }
                    }
                ]
            };

            mockTrainingProviderService.Setup(s => s.GetAllApplications(new List<Guid> { applicationId }))
                .ReturnsAsync(applicationsResponse);

            // Act
            var result = await vacancyClient.GetVacancyApplicationsSortedAsync(vacancyReference, SortColumn.DateApplied, SortOrder.Ascending);

            // Assert
            result.Should().ContainSingle();
            result[0].CandidateAppliedLocations.Should().NotBeNull();
        }

        [Test, MoqAutoData]
        public async Task GetVacancyApplicationsSortedAsync_HandlesJsonException_AndSetsNullLocation(long vacancyReference,
            Address address,
            ApplicationReview applicationReview,
            [Frozen] Mock<IApplicationReviewRepository> mockAppReviewRepo,
            [Frozen] Mock<ITrainingProviderService> mockTrainingProviderService,
            [Greedy] VacancyClient vacancyClient)
        {
            var applicationId = Guid.NewGuid();
            applicationReview.Application.ApplicationId = applicationId;
            applicationReview.Id = applicationId;
            applicationReview.IsWithdrawn = false;

            mockAppReviewRepo.Setup(r => r.GetForVacancySortedAsync(It.IsAny<long>(), It.IsAny<SortColumn>(), It.IsAny<SortOrder>()))
                .ReturnsAsync([applicationReview]);

            var applicationsResponse = new GetAllApplicationsResponse
            {
                Applications =
                [
                    new GetAllApplicationsResponse.Application
                    {
                        Id = applicationId,
                        EmploymentLocation = new GetAllApplicationsResponse.Location
                        {
                            Addresses =
                            [
                                new GetAllApplicationsResponse.Address
                                {
                                    AddressOrder = 1,
                                    FullAddress = "Invalid Json",
                                    IsSelected = true
                                }
                            ]
                        }
                    }
                ]
            };

            mockTrainingProviderService.Setup(s => s.GetAllApplications(new List<Guid> { applicationId }))
                .ReturnsAsync(applicationsResponse);

            // Act
            var result = await vacancyClient.GetVacancyApplicationsSortedAsync(vacancyReference, SortColumn.DateApplied, SortOrder.Ascending);

            // Assert
            result.Should().ContainSingle();
            result[0].CandidateAppliedLocations.Should().BeNullOrEmpty();
        }
    }
}