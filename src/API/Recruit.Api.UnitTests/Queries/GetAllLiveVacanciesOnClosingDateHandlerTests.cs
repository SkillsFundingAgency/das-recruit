using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Helpers;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Recruit.Api.UnitTests.Queries;

public class GetAllLiveVacanciesOnClosingDateHandlerTests
{
    [Test, MoqAutoData]
    public async Task Then_The_Live_Vacancies_Are_Returned_By_Date(byte pageSize,
        uint pageNumber,
        int vacanciesCount,
        GetLiveVacanciesOnDateQuery query,
        List<LiveVacancy> liveVacancies,
        [Frozen] Mock<IQueryStoreReader> queryStoreReader,
        GetLiveVacanciesOnDateQueryHandler handler
    )
    {
        liveVacancies.ForEach(x => x.Wage.WageType = WageType.FixedWage.ToString());

        query.PageSize = pageSize;
        query.PageNumber = (int)pageNumber;
        queryStoreReader.Setup(x => x.GetAllLiveVacanciesOnClosingDate(query.PageSize * (query.PageNumber - 1), query.PageSize, query.ClosingDate)).ReturnsAsync(liveVacancies);
        queryStoreReader.Setup(x => x.GetAllLiveVacanciesOnClosingDateCount(query.ClosingDate)).ReturnsAsync(vacanciesCount);
        
        var actual = await handler.Handle(query, CancellationToken.None);
        
        AssertResponse(vacanciesCount, query, liveVacancies, actual);
        
    }
    
    [Test, MoqAutoData]
    public async Task Then_If_Requested_PageSize_Is_Greater_Than_1000_Then_Limited_To_1000(
        uint pageNumber,
        int vacanciesCount,
        GetLiveVacanciesOnDateQuery query,
        List<LiveVacancy> liveVacancies,
        [Frozen] Mock<IQueryStoreReader> queryStoreReader,
        GetLiveVacanciesOnDateQueryHandler handler)
    {
        liveVacancies.ForEach(x => x.Wage.WageType = WageType.FixedWage.ToString());

        query.PageSize = 1001;
        query.PageNumber = (int)pageNumber;
        queryStoreReader.Setup(x => x.GetAllLiveVacanciesOnClosingDate(1000 * (query.PageNumber - 1), 1000, query.ClosingDate)).ReturnsAsync(liveVacancies);
        queryStoreReader.Setup(x => x.GetAllLiveVacanciesOnClosingDateCount(query.ClosingDate)).ReturnsAsync(vacanciesCount);
        
        var actual = await handler.Handle(query, CancellationToken.None);
        
        AssertResponse(vacanciesCount, query, liveVacancies, actual);
    }

    [Test, MoqAutoData]
    public async Task And_No_Live_Vacancies_Found_For_Date_Then_Success_Is_Returned_And_Empty_List(
        GetLiveVacanciesOnDateQuery query,
        [Frozen] Mock<IQueryStoreReader> mockQueryStoreReader,
        GetLiveVacanciesOnDateQueryHandler handler)
    {
        mockQueryStoreReader.Setup(x => x.GetAllLiveVacanciesOnClosingDate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).ReturnsAsync(() => null);
        
        var actual = await handler.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            actual.ResultCode.Should().Be(ResponseCode.Success);
            actual.Data.Should().BeEquivalentTo(Enumerable.Empty<LiveVacancy>());
        }
    }
    
    private static void AssertResponse(int vacanciesCount, GetLiveVacanciesOnDateQuery query, List<LiveVacancy> liveVacancies,
        GetLiveVacanciesOnDateQueryResult actual)
    {
        actual.Data.Should().BeOfType<LiveVacanciesSummary>();
        actual.Data.As<LiveVacanciesSummary>().Vacancies.Should().BeEquivalentTo(liveVacancies);
        actual.Data.As<LiveVacanciesSummary>().PageNo.Should()
            .Be(PagingHelper.GetRequestedPageNo(query.PageNumber, query.PageSize, vacanciesCount));
        actual.Data.As<LiveVacanciesSummary>().PageSize.Should().Be(query.PageSize);
        actual.Data.As<LiveVacanciesSummary>().TotalLiveVacancies.Should().Be(vacanciesCount);
        actual.Data.As<LiveVacanciesSummary>().TotalPages.Should()
            .Be(PagingHelper.GetTotalNoOfPages(query.PageSize, vacanciesCount));
        actual.ResultCode.Should().Be(ResponseCode.Success);
    }
}