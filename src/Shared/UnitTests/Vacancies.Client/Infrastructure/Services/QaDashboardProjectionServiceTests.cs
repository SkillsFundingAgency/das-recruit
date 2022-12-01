using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class QaDashboardProjectionServiceTests
    {
        [Test, MoqAutoData]
        public async Task When_Rebuilding_The_Dashboard(
            [Frozen] Mock<ITimeProvider> timeProvider,
            [Frozen] Mock<IVacancyReviewQuery> vacancyReviewQuery,
            [Frozen] Mock<IQueryStoreWriter> queryStoreWriter,
            QaDashboardProjectionService projectionService)
        {
            timeProvider.Setup(x => x.Now).Returns(DateTime.Now);
            var returnedVacancies = new List<VacancyReviewSummary>
            {
                new VacancyReviewSummary
                {
                    VacancyReference = 1,
                    SubmissionCount = 2,
                    CreatedDate = DateTime.Now.AddDays(-1)
                },
                new VacancyReviewSummary
                {
                    VacancyReference = 2,
                    SubmissionCount = 2,
                    CreatedDate = DateTime.Now.AddDays(-1)
                },
                new VacancyReviewSummary
                {
                    VacancyReference = 3,
                    SubmissionCount = 1,
                    SlaDeadline = DateTime.Now.AddMinutes(-1),
                    CreatedDate = DateTime.Now.AddDays(-1).AddMinutes(-10)
                },
                new VacancyReviewSummary
                {
                    VacancyReference = 4,
                    SubmissionCount = 1,
                    CreatedDate = DateTime.Now.AddHours(-12).AddMinutes(-10)
                },
                new VacancyReviewSummary
                {
                    VacancyReference = 5,
                    SubmissionCount = 1,
                    CreatedDate = DateTime.Now.AddHours(-23)
                },
                new VacancyReviewSummary
                {
                    VacancyReference = 6,
                    SubmissionCount = 1,
                    CreatedDate = DateTime.Now.AddHours(-24).AddMinutes(-1)
                },
                new VacancyReviewSummary
                {
                    VacancyReference = 7,
                    SubmissionCount = 1
                }
            };
            vacancyReviewQuery.Setup(x => x.GetActiveAsync<VacancyReviewSummary>()).ReturnsAsync(returnedVacancies);
            
            await projectionService.RebuildQaDashboardAsync();
            
            queryStoreWriter.Verify(x=>x.UpdateQaDashboardAsync(It.Is<QaDashboard>(c=>
                c.TotalVacanciesResubmitted.Equals(2)
                && c.TotalVacanciesSubmittedTwelveTwentyFourHours.Equals(2)
                && c.TotalVacanciesBrokenSla.Equals(1)
                && c.TotalVacanciesForReview.Equals(7)
            )), Times.Once);
        }
    }
}