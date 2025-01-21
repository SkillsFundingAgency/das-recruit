﻿using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
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
    public class GetClosedVacancyQueryHandlerTests
    {
        [Test]
        [MoqInlineAutoData(VacancyStatus.Closed)]
        [MoqInlineAutoData(VacancyStatus.Live)]
        public async Task Then_The_Closed_Vacancy_Is_Returned(
            VacancyStatus status,
            GetClosedVacancyQuery query,
            Vacancy closedVacancy,
            [Frozen] Mock<IVacancyQuery> queryStoreReader,
            GetClosedVacancyQueryHandler handler)
        {
            closedVacancy.Status = status;
            queryStoreReader.Setup(x => x.GetVacancyAsync(It.Is<long>(r => r == query.VacancyReference)))
                .ReturnsAsync(closedVacancy);

            var actual = await handler.Handle(query, CancellationToken.None);

            actual.Data.Should().BeEquivalentTo(closedVacancy);
            actual.ResultCode = ResponseCode.Success;
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_The_Vacancy_Is_Not_Closed_Null_Returned(
            Vacancy vacancy,
            GetClosedVacancyQuery query,
            [Frozen] Mock<IVacancyQuery> queryStoreReader,
            GetClosedVacancyQueryHandler handler)
        {
            vacancy.Status = VacancyStatus.Draft;
            queryStoreReader.Setup(x => x.GetVacancyAsync(It.Is<long>(r => r == query.VacancyReference)))
                .ReturnsAsync(vacancy);
            
            var actual = await handler.Handle(query, CancellationToken.None);

            actual.Data.Should().BeNull();
            actual.ResultCode = ResponseCode.NotFound;
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_The_QueryStore_Throws_VacancyNotFoundException_Null_Returned(
            GetClosedVacancyQuery query,
            [Frozen] Mock<IVacancyQuery> queryStoreReader,
            GetClosedVacancyQueryHandler handler)
        {
            queryStoreReader.Setup(x => x.GetVacancyAsync(It.Is<long>(r => r == query.VacancyReference)))
                .ThrowsAsync(new VacancyNotFoundException("Vacancy not found"));
            
            var actual = await handler.Handle(query, CancellationToken.None);

            actual.Data.Should().BeNull();
            actual.ResultCode = ResponseCode.NotFound;
        }
    }
}
