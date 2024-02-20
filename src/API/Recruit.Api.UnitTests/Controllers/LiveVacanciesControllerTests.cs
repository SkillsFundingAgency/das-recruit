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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Recruit.Api.UnitTests.Controllers;
public class LiveVacanciesControllerTests
{
    [Test, MoqAutoData]
    public async Task When_Getting_Live_Vacancies_Then_Query_Is_Created_And_Live_Vacancies_Returned(
    List<LiveVacancy> items,
    GetLiveVacanciesQueryResponse response,
    [Frozen] Mock<IMediator> mockMediator,
    [Greedy] LiveVacanciesController controller)
    {
        response.Data = items;
        mockMediator
            .Setup(x => x.Send(It.IsAny<GetLiveVacanciesQuery>(), CancellationToken.None))
            .ReturnsAsync(response);

        var actual = await controller.Get() as OkObjectResult;

        using (new AssertionScope())
        {
            Assert.That(actual, Is.Not.Null);
            var actualResult = actual.Value as List<LiveVacancy>;
            actualResult.Should().BeEquivalentTo(items);
        }
    }

    [Test, MoqAutoData]
    public async Task When_Getting_Live_Vacancy_Then_Query_Is_Created_And_Live_Vacancy_Returned(
        long vacancyId,
        LiveVacancy vacancy,
        GetLiveVacancyQueryResponse response,
        [Frozen] Mock<IMediator> mockMediator,
        [Greedy] LiveVacanciesController controller)
    {
        response.Data = vacancy;
        mockMediator
            .Setup(x => x.Send(It.Is<GetLiveVacancyQuery>(q => q.VacancyReference == vacancyId), CancellationToken.None))
            .ReturnsAsync(response);

        var actual = await controller.Get(vacancyId) as OkObjectResult;

        using (new AssertionScope())
        {
            Assert.That(actual, Is.Not.Null);
            var actualResult = actual.Value as LiveVacancy;
            actualResult.Should().BeEquivalentTo(vacancy);
        }
    }
}
