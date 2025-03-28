using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Recruit.Api.UnitTests.Queries
{
    [TestFixture]
    public class GetLiveVacancyQueryHandlerTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_Live_Vacancy_Is_Returned(
            GetLiveVacancyQuery query,
            LiveVacancy liveVacancy,
            [Frozen] Mock<IQueryStoreReader> queryStoreReader,
            GetLiveVacancyQueryHandler handler)
        {
            liveVacancy.Wage.WageType = WageType.FixedWage.ToString();

            queryStoreReader.Setup(x => x.GetLiveVacancy(It.Is<long>(r => r == query.VacancyReference)))
                .ReturnsAsync(liveVacancy);

            var actual = await handler.Handle(query, CancellationToken.None);

            actual.Data.Should().BeEquivalentTo(liveVacancy);
            actual.ResultCode = ResponseCode.Success;
        }
    }
}
