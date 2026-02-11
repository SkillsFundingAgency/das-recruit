using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Vacancies;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Vacancies;

public class WhenListingVacancies
{
    [Test]
    [MoqInlineAutoData(FilteringOptions.All, typeof(GetAllVacanciesByUkprnApiRequest))]
    [MoqInlineAutoData(FilteringOptions.Draft, typeof(GetDraftVacanciesByUkprnApiRequest))]
    [MoqInlineAutoData(FilteringOptions.Review, typeof(GetReviewVacanciesByUkprnApiRequest))]
    public async Task Then_The_List_Vacancies_Query_Is_Constructed_Correctly(
        FilteringOptions filteringOption,
        Type expectedType,
        int ukprn,
        string userId,
        GetAlertsByUkprnApiResponse alertsResponse,
        PagedDataResponse<IEnumerable<VacancyListItem>> vacanciesResponse,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        VacanciesOrchestrator sut)
    {
        // arrange
        GetVacanciesByUkprnApiRequestV2? capturedRequest = null;
        outerApiClient
            .Setup(x => x.Get<PagedDataResponse<IEnumerable<VacancyListItem>>>(It.IsAny<GetVacanciesByUkprnApiRequestV2>()))
            .Callback((IGetApiRequest x) => capturedRequest = x as GetVacanciesByUkprnApiRequestV2)
            .ReturnsAsync(vacanciesResponse);

        // act
        await sut.ListVacanciesAsync(filteringOption, ukprn, userId, "foo", 5, 50, VacancySortColumn.ClosingDate, ColumnSortOrder.Asc);

        // assert
        capturedRequest.Should().NotBeNull();
        capturedRequest.GetType().Should().Be(expectedType);
        capturedRequest.Page.Should().Be(5);
        capturedRequest.PageSize.Should().Be(50);
        capturedRequest.SearchTerm.Should().Be("foo");
        capturedRequest.SortColumn.Should().Be(VacancySortColumn.ClosingDate);
        capturedRequest.SortOrder.Should().Be(ColumnSortOrder.Asc);
        capturedRequest.Ukprn.Should().Be(ukprn);
    }
    
    [Test]
    [MoqInlineAutoData(FilteringOptions.All, "All vacancies")]
    [MoqInlineAutoData(FilteringOptions.Draft, "Draft vacancies")]
    [MoqInlineAutoData(FilteringOptions.Review, "Pending employer review")]
    public async Task Then_The_View_Is_Constructed_Correctly(
        FilteringOptions filteringOption,
        string expectedPageHeading,
        int ukprn,
        string userId,
        List<VacancyListItem> vacancyListItems,
        GetAlertsByUkprnApiResponse alertsResponse,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        VacanciesOrchestrator sut)
    {
        // arrange
        var vacanciesResponse = new PagedDataResponse<IEnumerable<VacancyListItem>>(vacancyListItems, new PageInfo(5, 50, (uint)vacancyListItems.Count)); 
        outerApiClient
            .Setup(x => x.Get<PagedDataResponse<IEnumerable<VacancyListItem>>>(It.IsAny<GetVacanciesByUkprnApiRequestV2>()))
            .ReturnsAsync(vacanciesResponse);

        // act
        var result = await sut.ListVacanciesAsync(filteringOption, ukprn, userId, "foo", 5, 50, VacancySortColumn.ClosingDate, ColumnSortOrder.Asc);

        // assert
        result.Should().NotBeNull();
        result.Alerts.TransferredVacanciesAlert.Should().BeNull();
        result.Ukprn.Should().Be(ukprn);
        result.PageHeading.Should().Be(expectedPageHeading);
        result.FilterViewModel.SearchTerm.Should().Be("foo");
        result.FilterViewModel.UserType.Should().Be(UserType.Provider);
        result.ListViewModel.UserType.Should().Be(UserType.Provider);
        result.ListViewModel.SortColumn.Should().Be(VacancySortColumn.ClosingDate);
        result.ListViewModel.SortOrder.Should().Be(ColumnSortOrder.Asc);
        result.ListViewModel.Vacancies.Count.Should().Be(vacancyListItems.Count);
        result.ListViewModel.RouteDictionary.Count.Should().Be(4);
        result.ListViewModel.RouteDictionary.Keys.Should().Contain(["searchTerm", "sortOrder", "sortColumn", "ukprn"]);
        result.ListViewModel.Pagination.TotalPages.Should().Be(1);
        result.ListViewModel.Pagination.CurrentPage.Should().Be(5);
    }
}