using System;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.Recruit.Api.Controllers;
using Xunit;
using Moq;
using MediatR;
using SFA.DAS.Recruit.Api.Queries;
using System.Threading;

namespace SFA.DAS.Recruit.Api.UnitTests.Controllers
{
    public class VacanciesControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly VacanciesController _sut;
        private GetVacanciesQuery _queryPassed;

        public VacanciesControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(x => x.Send(It.IsAny<GetVacanciesQuery>(), CancellationToken.None))
                        .ReturnsAsync(new GetVacanciesResponse())
                        .Callback<IRequest<GetVacanciesResponse>, CancellationToken>((q, _) => _queryPassed = (GetVacanciesQuery)q);
            _sut = new VacanciesController(_mockMediator.Object);
        }

        [Theory]
        [InlineData(" myjr4x")]
        [InlineData("MYJR4X")]
        [InlineData(" myjR4X ")]
        public async Task GetCall_EnsuresEmployerAccountIdPassedInQueryPassedToMediatorIsTrimmedAndUppercased(string input)
        {
            await _sut.Get(input, 0, 0, 25, 1);
            string.CompareOrdinal(_queryPassed.EmployerAccountId, "MYJR4X").Should().Be(0);
        }
    }
}