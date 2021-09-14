using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.Recruit.Api.Controllers;
using Xunit;
using Moq;
using MediatR;
using SFA.DAS.Recruit.Api.Queries;
using System.Threading;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Commands;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Testing.AutoFixture;
using Assert = Xunit.Assert;

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

        [Xunit.Theory]
        [InlineData(" myjr4x")]
        [InlineData("MYJR4X")]
        [InlineData(" myjR4X ")]
        public async Task GetCall_EnsuresEmployerAccountIdPassedInQueryPassedToMediatorIsTrimmedAndUppercased(string input)
        {
            await _sut.Get(input, 0, 0, 25, 1);
            string.CompareOrdinal(_queryPassed.EmployerAccountId, "MYJR4X").Should().Be(0);
        }

        [Test, MoqAutoData]
        public async Task CreateVacancy_Then_The_Request_Is_Sent_To_Mediator_Command(
            Guid id,
            long vacancyRef,
            string userEmail,
            CreateVacancyRequest request,
            CreateVacancyCommandResponse response,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] VacanciesController controller)
        {
            response.ResultCode = ResponseCode.Created;
            response.Data = vacancyRef; 
            mediator.Setup(x => x.Send(It.Is<CreateVacancyCommand>(c => 
                    c.Vacancy.Title.Equals(request.Title) 
                    && c.Vacancy.Id.Equals(id)
                    && c.Ukprn.Equals(request.Ukprn)
                    && c.CreatedByUser.Email.Equals(userEmail)
                    ), CancellationToken.None)).ReturnsAsync(response);

            var actual = await controller.Create(id, request, userEmail) as CreatedResult;

            Assert.NotNull(actual);
            actual.StatusCode.Should().Be((int) HttpStatusCode.Created);
            var actualResult = actual.Value as long?;
            Assert.NotNull(actualResult);
            actualResult.Value.Should().Be((long)response.Data);
        }
    }
}