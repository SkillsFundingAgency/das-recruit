using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using FluentAssertions;
using FluentAssertions.Execution;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Controllers;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Testing.AutoFixture;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Recruit.Api.UnitTests.Controllers;
public class ClosedVacanciesControllerTests
{
    [Test, MoqAutoData]
    public async Task When_Getting_Closed_Vacancy_Then_Query_Is_Created_And_Closed_Vacancy_Returned(
        long vacancyId,
        ClosedVacancy vacancy,
        GetClosedVacancyQueryResponse response,
        [Frozen] Mock<IMediator> mockMediator,
        [Greedy] ClosedVacanciesController controller)
    {
        response.Data = vacancy;
        mockMediator
            .Setup(x => x.Send(It.Is<GetClosedVacancyQuery>(q => q.VacancyReference == vacancyId), CancellationToken.None))
            .ReturnsAsync(response);

        var actual = await controller.Get(vacancyId) as OkObjectResult;

        using (new AssertionScope())
        {
            Assert.That(actual, Is.Not.Null);
            var actualResult = actual.Value as ClosedVacancy;
            actualResult.Should().BeEquivalentTo(vacancy);
        }
    }
}
