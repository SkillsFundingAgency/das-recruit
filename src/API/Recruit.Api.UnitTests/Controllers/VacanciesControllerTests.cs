using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SFA.DAS.Recruit.Api.Controllers;
using MediatR;
using SFA.DAS.Recruit.Api.Queries;
using System.Threading;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Recruit.Api.Commands;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Testing.AutoFixture;
using Assert = NUnit.Framework.Assert;

namespace SFA.DAS.Recruit.Api.UnitTests.Controllers
{
    public class GetVacanciesControllerTests
    {
        private Mock<IMediator> _mockMediator;
        private VacanciesController _sut;
        private GetVacanciesQuery _queryPassed;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(x => x.Send(It.IsAny<GetVacanciesQuery>(), CancellationToken.None))
                .ReturnsAsync(new GetVacanciesResponse())
                .Callback<IRequest<GetVacanciesResponse>, CancellationToken>((q, _) => _queryPassed = (GetVacanciesQuery)q);
            _sut = new VacanciesController(_mockMediator.Object);
        }

        
        [TestCase(" myjr4x")]
        [TestCase("MYJR4X")]
        [TestCase(" myjR4X ")]
        public async Task GetCall_EnsuresEmployerAccountIdPassedInQueryPassedToMediatorIsTrimmedAndUppercased(string input)
        {
            await _sut.Get(input, 0,  25, 1);
            string.CompareOrdinal(_queryPassed.EmployerAccountId, "MYJR4X").Should().Be(0);
        }
        
        [Test, MoqAutoData]
        public async Task CreateVacancy_Then_The_Request_Is_Sent_To_Mediator_Command(
            Guid id,
            long vacancyRef,
            string userEmail,
            long ukprn,
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
                    && c.VacancyUserDetails.Email.Equals(userEmail)
                    && c.VacancyUserDetails.Ukprn.Equals(ukprn)
                    && !c.ValidateOnly
                    ), CancellationToken.None)).ReturnsAsync(response);

            var actual = await controller.Create(id, request, userEmail, ukprn) as CreatedResult;

            Assert.That(actual, Is.Not.Null);
            actual.StatusCode.Should().Be((int)HttpStatusCode.Created);
            var actualResult = actual.Value as long?;
            Assert.That(actualResult, Is.Not.Null);
            actualResult.Value.Should().Be((long)response.Data);
        }

        [Test, MoqAutoData]
        public async Task ValidateVacancy_Then_The_Request_Is_Sent_To_Mediator_Command(
            Guid id,
            long vacancyRef,
            string userEmail,
            long ukprn,
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
                    && c.VacancyUserDetails.Email.Equals(userEmail)
                    && c.VacancyUserDetails.Ukprn.Equals(ukprn)
                    && c.ValidateOnly
                ), CancellationToken.None))
                .ReturnsAsync(response);

            var actual = await controller.Validate(id, request, userEmail, ukprn) as CreatedResult;

            Assert.That(actual, Is.Not.Null);
            actual.StatusCode.Should().Be((int)HttpStatusCode.Created);
            var actualResult = actual.Value as long?;
            Assert.That(actualResult, Is.Not.Null);
            actualResult.Value.Should().Be((long)response.Data);
        }

        [Test]
        [MoqInlineAutoData("EmployerLocation", "Address")]
        [MoqInlineAutoData("EmployerLocations", "Addresses")]
        [MoqInlineAutoData("EmployerLocations[0].Country", "Addresses[0].Country")]
        [MoqInlineAutoData("EmployerLocations[3].Country", "Addresses[3].Country")]
        public async Task Create_Maps_Error_Fields(
            string fieldName,
            string expectedFieldName,
            Guid id,
            long vacancyRef,
            string userEmail,
            long ukprn,
            CreateVacancyRequest request,
            CreateVacancyCommandResponse response,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] VacanciesController controller)
        {
            // arrange
            mediator.Setup(x => x.Send(It.IsAny<CreateVacancyCommand>(), CancellationToken.None)).ReturnsAsync(response);
            response.ResultCode = ResponseCode.InvalidRequest;
            response.ValidationErrors = [new DetailedValidationError { Field = fieldName, Message = "Validation message" }];
            var expected = new { Errors = new List<object> { new DetailedValidationError { Field = expectedFieldName, Message = "Validation message" } } };
            
            // act
            var actual = await controller.Create(id, request, userEmail, ukprn) as BadRequestObjectResult;
            
            // assert
            actual.Should().NotBeNull();
            actual!.Value.Should().NotBeNull();
            actual.Value.Should().BeEquivalentTo(expected);
        }
    }
}