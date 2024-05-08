using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Recruit.Api.UnitTests.Queries
{
    [TestFixture]
    public class GetClosedVacancyQueryHandlerTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_Closed_Vacancy_Is_Returned(
            GetClosedVacancyQuery query,
            LiveVacancy closedVacancy,
            [Frozen] Mock<IQueryStoreReader> queryStoreReader,
            GetClosedVacancyQueryHandler handler)
        {
            queryStoreReader.Setup(x => x.GetLiveExpiredVacancy(It.Is<long>(r => r == query.VacancyReference)))
                .ReturnsAsync(closedVacancy);

            var actual = await handler.Handle(query, CancellationToken.None);

            actual.Data.Should().BeEquivalentTo(closedVacancy);
            actual.ResultCode = ResponseCode.Success;
        }
        [Test, MoqAutoData]
        public async Task Then_If_The_LiveExpired_Vacancy_Is_Not_Returned_Closed_Vacancy_Is_Returned(
            GetClosedVacancyQuery query,
            ClosedVacancy closedVacancy,
            [Frozen] Mock<IQueryStoreReader> queryStoreReader,
            GetClosedVacancyQueryHandler handler)
        {
            queryStoreReader.Setup(x => x.GetLiveExpiredVacancy(It.Is<long>(r => r == query.VacancyReference)))
                .ReturnsAsync((LiveVacancy)null);
            queryStoreReader.Setup(x => x.GetClosedVacancy(It.Is<long>(r => r == query.VacancyReference)))
                .ReturnsAsync(closedVacancy);
            
            var actual = await handler.Handle(query, CancellationToken.None);

            actual.Data.Should().BeEquivalentTo(closedVacancy);
            actual.ResultCode = ResponseCode.Success;
        }
    }
}
