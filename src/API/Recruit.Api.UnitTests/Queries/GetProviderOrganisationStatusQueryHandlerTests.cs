using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Recruit.Api.Services;
using Xunit;

namespace SFA.DAS.Recruit.Api.UnitTests.Queries
{
    public class GetProviderOrganisationStatusQueryHandlerTests
    {
        private const long ValidUkprn = 10000020;
        private const long BlockedUkprn = 11110000;
        private readonly Mock<IQueryStoreReader> _mockQueryStoreReader;
        private readonly GetProviderOrganisationStatusQueryHandler _sut;

        public GetProviderOrganisationStatusQueryHandlerTests()
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

        [Fact]
        public async Task GivenRequestWithBlockedProviderUkprn_ShouldReturnBlockedStatus()
        {
            var query = new GetProviderOrganisationStatusQuery(BlockedUkprn);
            var result = await _sut.Handle(query, CancellationToken.None);
            result.ResultCode.Should().Be(ResponseCode.Success);
            result.Data.As<OrganisationStatus>().Status.Should().Be("Blocked");
        }

        [Fact]
        public async Task GivenRequestWithUnblockedProviderUkprn_ShouldReturnUnblockedStatus()
        {
            var query = new GetProviderOrganisationStatusQuery(11111111);
            var result = await _sut.Handle(query, CancellationToken.None);
            result.ResultCode.Should().Be(ResponseCode.Success);
            result.Data.As<OrganisationStatus>().Status.Should().Be("Not Blocked");
        }
    }
}