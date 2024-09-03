using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Controllers;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Recruit.Api.UnitTests.Controllers
{
    public class ReferenceDataControllerTests
    {
        [Test, MoqAutoData]
        public async Task When_Getting_Skills_Then_The_Query_Is_Made_And_Data_Returned(
            List<string> items,
            GetSkillsQueryResponse response,
            [Frozen] Mock<IMediator> mockMediator,
            [Greedy] ReferenceDataController controller)
        {
            response.Data = items;
            mockMediator
                .Setup(x => x.Send(It.IsAny<GetSkillsQuery>(), CancellationToken.None))
                .ReturnsAsync(response);

            
            var actual = await controller.GetCandidateSkills() as OkObjectResult;

            Assert.That(actual, Is.Not.Null);
            var actualResult = actual.Value as List<string>;
            actualResult.Should().BeEquivalentTo(items);
        }

        [Test, MoqAutoData]
        public async Task When_Getting_Qualifications_Then_The_Query_Is_Made_And_Data_Returned(
            List<string> items,
            GetQualificationsQueryResponse response,
            [Frozen] Mock<IMediator> mockMediator,
            [Greedy] ReferenceDataController controller)
        {
            response.Data = items;
            mockMediator
                .Setup(x => x.Send(It.IsAny<GetQualificationsQuery>(), CancellationToken.None))
                .ReturnsAsync(response);

            var actual = await controller.GetCandidateQualifications() as OkObjectResult;

            Assert.That(actual, Is.Not.Null);
            var actualResult = actual.Value as List<string>;
            actualResult.Should().BeEquivalentTo(items);
        }

    }
}