using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using SFA.DAS.Recruit.Api.Controllers;
using SFA.DAS.Recruit.Api.Queries;
using Xunit;

namespace SFA.DAS.Recruit.Api.UnitTests.Controllers
{
    public class OrganisationStatusControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly OrganisationStatusController _sut;
        private GetEmployerOrganisationStatusQuery _queryPassed;

        public OrganisationStatusControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(x => x.Send(It.IsAny<GetEmployerOrganisationStatusQuery>(), CancellationToken.None))
                        .ReturnsAsync(new GetOrganisationStatusResponse())
                        .Callback<IRequest<GetOrganisationStatusResponse>, CancellationToken>((q, _) => _queryPassed = (GetEmployerOrganisationStatusQuery)q);
            _sut = new OrganisationStatusController(_mockMediator.Object);
        }

        [Theory]
        [InlineData(" myjr4x")]
        [InlineData("MYJR4X")]
        [InlineData(" myjR4X ")]
        public async Task GetCall_EnsuresEmployerAccountIdPassedInQueryPassedToMediatorIsTrimmedAndUppercased(string input)
        {
            var result = await _sut.Get(input);
            string.Compare(_queryPassed.EmployerAccountId, "MYJR4X", false).Should().Be(0);
        }
    }
}