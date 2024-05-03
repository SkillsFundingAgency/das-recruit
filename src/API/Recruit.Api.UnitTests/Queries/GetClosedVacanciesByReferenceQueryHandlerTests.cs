using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Recruit.Api.UnitTests.Queries
{
    [TestFixture]
    public class GetClosedVacanciesByReferenceQueryHandlerTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_Closed_Vacancies_Are_Returned(
            GetClosedVacanciesByReferenceQuery query,
            List<ClosedVacancy> closedVacancies,
            [Frozen] Mock<IQueryStoreReader> queryStoreReader,
            GetClosedVacanciesByReferenceQueryHandler handler)
        {
            queryStoreReader.Setup(x => x.GetClosedVacancies(It.Is<List<long>>(r => r == query.VacancyReferences)))
                .ReturnsAsync(closedVacancies);

            var actual = await handler.Handle(query, CancellationToken.None);

            actual.Data.Should().BeEquivalentTo(new ClosedVacanciesSummary{Vacancies = closedVacancies });
            actual.ResultCode = ResponseCode.Success;
        }
    }
}
