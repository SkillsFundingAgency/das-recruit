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
using Esfa.Recruit.Vacancies.Client.Application.Configuration;

namespace SFA.DAS.Recruit.Api.UnitTests.Controllers
{
    public class TraineeshipVacanciesControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly VacanciesController _sut;
        private GetVacanciesQuery _queryPassed;

        public TraineeshipVacanciesControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(x => x.Send(It.IsAny<GetVacanciesQuery>(), CancellationToken.None))
                        .ReturnsAsync(new GetVacanciesResponse())
                        .Callback<IRequest<GetVacanciesResponse>, CancellationToken>((q, _) => _queryPassed = (GetVacanciesQuery)q);
            _sut = new VacanciesController(_mockMediator.Object, new ServiceParameters(VacancyType.Traineeship.ToString()));
        }

        [Test, MoqAutoData]
        public async Task CreateTraineeeshipVacancy_Then_The_Request_Is_Sent_To_Mediator_Command(
            Guid id,
            long vacancyRef,
            string userEmail,
            long ukprn,
            CreateTraineeshipVacancyRequest request,
            CreateTraineeshipVacancyCommandResponse response,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] VacanciesController controller)
        {
            response.ResultCode = ResponseCode.Created;
            response.Data = vacancyRef;
            mediator.Setup(x => x.Send(It.Is<CreateTraineeshipVacancyCommand>(c =>
                    c.Vacancy.Title.Equals(request.Title)
                    && c.Vacancy.Id.Equals(id)
                    && c.VacancyUserDetails.Email.Equals(userEmail)
                    && c.VacancyUserDetails.Ukprn.Equals(ukprn)
                    && !c.ValidateOnly
                    ), CancellationToken.None)).ReturnsAsync(response);

            var actual = await controller.CreateTraineeship(id, request, userEmail, ukprn) as CreatedResult;

            Assert.NotNull(actual);
            actual.StatusCode.Should().Be((int)HttpStatusCode.Created);
            var actualResult = actual.Value as long?;
            Assert.NotNull(actualResult);
            actualResult.Value.Should().Be((long)response.Data);
        }
    }
}