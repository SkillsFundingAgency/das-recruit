using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Vacancies;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Vacancies;

public class WhenListingVacancies
{
    [Test]
    [MoqInlineAutoData(FilteringOptions.All, typeof(GetAllVacanciesByEmployerAccountApiRequest))]
    [MoqInlineAutoData(FilteringOptions.Draft, typeof(GetDraftVacanciesByEmployerAccountApiRequest))]
    public async Task Then_The_List_Vacancies_Query_Is_Constructed_Correctly(
        FilteringOptions filteringOption,
        Type expectedType,
        string hashedEmployerAccountId,
        long employerAccountId,
        string userId,
        int? ukprn,
        GetAlertsByAccountIdApiResponse alertsResponse,
        PagedDataResponse<IEnumerable<VacancyListItem>> vacanciesResponse,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Frozen] Mock<IEncodingService> encodingService,
        VacanciesOrchestrator sut)
    {
        // arrange
        GetVacanciesByEmployerAccountApiRequestV2? capturedRequest = null;
        outerApiClient
            .Setup(x => x.Get<PagedDataResponse<IEnumerable<VacancyListItem>>>(It.IsAny<GetVacanciesByEmployerAccountApiRequestV2>()))
            .Callback((IGetApiRequest x) => capturedRequest = x as GetVacanciesByEmployerAccountApiRequestV2)
            .ReturnsAsync(vacanciesResponse);
        encodingService
            .Setup(x => x.Decode(hashedEmployerAccountId, EncodingType.AccountId))
            .Returns(employerAccountId);

        // act
        await sut.ListVacanciesAsync(filteringOption, hashedEmployerAccountId, ukprn, userId, "foo", 5, 50, VacancySortColumn.ClosingDate, ColumnSortOrder.Asc);

        // assert
        capturedRequest.Should().NotBeNull();
        capturedRequest.GetType().Should().Be(expectedType);
        capturedRequest.Page.Should().Be(5);
        capturedRequest.PageSize.Should().Be(50);
        capturedRequest.SearchTerm.Should().Be("foo");
        capturedRequest.SortColumn.Should().Be(VacancySortColumn.ClosingDate);
        capturedRequest.SortOrder.Should().Be(ColumnSortOrder.Asc);
        capturedRequest.EmployerAccountId.Should().Be(employerAccountId);
    }
    
    [Test]
    [MoqInlineAutoData(FilteringOptions.All, "All adverts")]
    [MoqInlineAutoData(FilteringOptions.Draft, "Draft adverts")]
    public async Task Then_The_View_Is_Constructed_Correctly(
        FilteringOptions filteringOption,
        string expectedPageHeading,
        string hashedEmployerAccountId,
        long employerAccountId,
        int ukprn,
        string userId,
        List<VacancyListItem> vacancyListItems,
        GetAlertsByAccountIdApiResponse alertsResponse,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Frozen] Mock<IEncodingService> encodingService,
        VacanciesOrchestrator sut)
    {
        // arrange
        var vacanciesResponse = new PagedDataResponse<IEnumerable<VacancyListItem>>(vacancyListItems, new PageInfo(5, 50, (uint)vacancyListItems.Count)); 
        outerApiClient
            .Setup(x => x.Get<PagedDataResponse<IEnumerable<VacancyListItem>>>(It.IsAny<GetVacanciesByEmployerAccountApiRequestV2>()))
            .ReturnsAsync(vacanciesResponse);
        encodingService
            .Setup(x => x.Decode(hashedEmployerAccountId, EncodingType.AccountId))
            .Returns(employerAccountId);

        // act
        var result = await sut.ListVacanciesAsync(filteringOption, hashedEmployerAccountId, ukprn, userId, "foo", 5, 50, VacancySortColumn.ClosingDate, ColumnSortOrder.Asc);

        // assert
        result.Should().NotBeNull();
        result.PageHeading.Should().Be(expectedPageHeading);
        result.FilterViewModel.SearchTerm.Should().Be("foo");
        result.FilterViewModel.UserType.Should().Be(UserType.Employer);
        result.ListViewModel.UserType.Should().Be(UserType.Employer);
        result.ListViewModel.SortColumn.Should().Be(VacancySortColumn.ClosingDate);
        result.ListViewModel.SortOrder.Should().Be(ColumnSortOrder.Asc);
        result.ListViewModel.Vacancies.Count.Should().Be(vacancyListItems.Count);
        result.ListViewModel.RouteDictionary.Count.Should().Be(4);
        result.ListViewModel.RouteDictionary.Keys.Should().Contain(["searchTerm", "sortOrder", "sortColumn", "employerAccountId"]);
        result.ListViewModel.Pagination.TotalPages.Should().Be(1);
        result.ListViewModel.Pagination.CurrentPage.Should().Be(5);
    }
}