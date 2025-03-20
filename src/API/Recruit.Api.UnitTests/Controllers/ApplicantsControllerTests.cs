using System.Threading.Tasks;
using SFA.DAS.Recruit.Api.Controllers;
using MediatR;
using SFA.DAS.Recruit.Api.Queries;
using System.Threading;

namespace SFA.DAS.Recruit.Api.UnitTests.Controllers
{
    public class ApplicantsControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly ApplicantsController _sut;
        private GetApplicantsQuery _queryPassed;

        public ApplicantsControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(x => x.Send(It.IsAny<GetApplicantsQuery>(), CancellationToken.None))
                        .ReturnsAsync(new GetApplicantsResponse())
                        .Callback<IRequest<GetApplicantsResponse>, CancellationToken>((q, _) => _queryPassed = (GetApplicantsQuery)q);
            _sut = new ApplicantsController(_mockMediator.Object);
        }

        [Test]
        public async Task GetCall_EnsuresApplicantApplicationOutcomeFilterPassedToMediatorIsTrimmed()
        {
            var result = await _sut.Get(10000001, " successful ");
            _queryPassed.ApplicantApplicationOutcomeFilter.Contains(" ").Should().BeFalse();
        }
    }
}