using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Recruit.Api.UnitTests.Queries
{
    [TestFixture]
    public class GetTotalPositionsAvailableQueryHandlerTests
    {
        [Test, MoqAutoData]
        public async Task Then_TotalPositionsAvailable_Is_Returned(
            GetTotalPositionsAvailableQuery query,
            long totalPositionsAvailable,
            [Frozen] Mock<IQueryStoreReader> queryStoreReader,
            GetTotalPositionsAvailableQueryHandler handler)
        {
            queryStoreReader.Setup(x => x.GetTotalPositionsAvailableCount()).ReturnsAsync(totalPositionsAvailable);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Data.Should().Be(totalPositionsAvailable);
        }
    }
}