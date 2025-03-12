using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Recruit.Api.Services;

namespace SFA.DAS.Recruit.Api.UnitTests.Queries
{
    public class GetProviderOrganisationStatusQueryHandlerTests
    {
        private const long BlockedUkprn = 11110000;
        private Mock<IQueryStoreReader> _mockQueryStoreReader;
        private GetProviderOrganisationStatusQueryHandler _sut;

        [SetUp]
        public void Setup()
        {
            _mockQueryStoreReader = new Mock<IQueryStoreReader>();
            _mockQueryStoreReader.Setup(qsr => qsr.GetBlockedProviders())
                .ReturnsAsync(new BlockedProviderOrganisations
                {
                    Id = $"{nameof(BlockedProviderOrganisations)}",
                    Data = new []
                    {
                        new BlockedOrganisationSummary { BlockedOrganisationId = BlockedUkprn.ToString() }
                    }
                });
            _sut = new GetProviderOrganisationStatusQueryHandler(Mock.Of<ILogger<GetProviderOrganisationStatusQueryHandler>>(), _mockQueryStoreReader.Object);
        }

        [Test]
        public async Task GivenRequestWithBlockedProviderUkprn_ShouldReturnBlockedStatus()
        {
            var query = new GetProviderOrganisationStatusQuery(BlockedUkprn);
            var result = await _sut.Handle(query, CancellationToken.None);
            result.ResultCode.Should().Be(ResponseCode.Success);
            result.Data.As<OrganisationStatus>().Status.Should().Be("Blocked");
        }

        [Test]
        public async Task GivenRequestWithUnblockedProviderUkprn_ShouldReturnUnblockedStatus()
        {
            var query = new GetProviderOrganisationStatusQuery(11111111);
            var result = await _sut.Handle(query, CancellationToken.None);
            result.ResultCode.Should().Be(ResponseCode.Success);
            result.Data.As<OrganisationStatus>().Status.Should().Be("Not Blocked");
        }
    }
}