using System.Threading.Tasks;
using SFA.DAS.Recruit.Api.Controllers;
using MediatR;
using SFA.DAS.Recruit.Api.Queries;
using System.Threading;

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

        [TestCase(" myjr4x")]
        [TestCase("MYJR4X")]
        [TestCase(" myjR4X ")]
        public async Task GetCall_EnsuresEmployerAccountIdPassedInQueryPassedToMediatorIsTrimmedAndUppercased(string input)
        {
            var result = await _sut.Get(input);
            string.Compare(_queryPassed.EmployerAccountId, "MYJR4X", false).Should().Be(0);
        }
    }
}