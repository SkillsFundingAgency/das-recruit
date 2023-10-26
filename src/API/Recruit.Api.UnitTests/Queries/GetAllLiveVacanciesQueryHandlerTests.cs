using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Testing.AutoFixture;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using FluentAssertions;
using FluentAssertions.Execution;
using AutoFixture;

namespace SFA.DAS.Recruit.Api.UnitTests.Queries;
public class GetAllLiveVacanciesQueryHandlerTests
{
    private GetLiveVacanciesQueryHandler _sut;
    private Mock<IQueryStoreReader> _mockQueryStoreReader;
    private IEnumerable<LiveVacancy> liveVacancies;

    public GetAllLiveVacanciesQueryHandlerTests()
    {
        _mockQueryStoreReader = new Mock<IQueryStoreReader>();

        var liveVacanciesSummaryFixture = new Fixture();
        liveVacancies = liveVacanciesSummaryFixture.Build<LiveVacancy>().CreateMany(10);

        _mockQueryStoreReader.Setup(x => x.GetAllLiveVacancies(10)).ReturnsAsync(liveVacancies);

        _sut = new GetLiveVacanciesQueryHandler(_mockQueryStoreReader.Object);
    }

    [Test]
    public async Task Then_The_Live_Vacancies_Are_Returned()
    {
        var query = new GetLiveVacanciesQuery(10, 1);
        var actual = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            actual.Data.Should().BeOfType<LiveVacanciesSummary>();
            actual.Data.As<LiveVacanciesSummary>().Vacancies.Should().BeEquivalentTo(liveVacancies);
            actual.Data.As<LiveVacanciesSummary>().PageNo.Should().Be(query.PageNumber);
            actual.Data.As<LiveVacanciesSummary>().PageSize.Should().Be(query.PageSize);
            actual.Data.As<LiveVacanciesSummary>().TotalResults.Should().Be(10);
            actual.Data.As<LiveVacanciesSummary>().TotalPages.Should().Be(1);
            actual.ResultCode.Should().Be(ResponseCode.Success);
        }
    }

    [Test, MoqAutoData]
    public async Task And_No_Live_Vacancies_Found_Then_Not_Found_Is_Returned(
            GetLiveVacanciesQuery query,
            [Frozen] Mock<IQueryStoreReader> mockQueryStoreReader)
    {
        mockQueryStoreReader.Setup(x => x.GetAllLiveVacancies(It.IsAny<int>())).ReturnsAsync(() => null);
        var handler = new GetLiveVacanciesQueryHandler(mockQueryStoreReader.Object);

        var actual = await handler.Handle(query, CancellationToken.None);

        actual.ResultCode.Should().Be(ResponseCode.NotFound);
    }
}
