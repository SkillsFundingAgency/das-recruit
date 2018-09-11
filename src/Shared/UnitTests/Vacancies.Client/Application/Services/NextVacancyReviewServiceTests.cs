using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.NextVacancyReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Services
{
    public class NextVacancyReviewServiceTests
    {
        [Theory]
        [InlineData("2018-08-20T17:00:00Z", "user A", "92a9b68b-4555-425d-92a1-5d8ea625cf5b")]
        [InlineData("2018-08-20T17:00:01Z", "user A", "7a636aad-a3a5-4293-bae6-5c5a8b199265")]
        [InlineData("2018-08-20T18:00:00Z", "user A", "c7267018-c54d-4aa4-a59d-bf4a46fa24be")]
        [InlineData("2018-08-20T16:00:00Z", "user C", "8353063d-bc67-4588-83d9-2281080f8112")]
        public void GetNextVacancyReviewAsync_ShouldReturnNextReview(string utcTimeNow, string userId, string expectedReviewId)
        {
            var timeProvider = new Mock<ITimeProvider>();
            timeProvider.Setup(t => t.Now).Returns(DateTime.Parse(utcTimeNow));
            var vacancyReviewRepository = new Mock<IVacancyReviewRepository>();
            vacancyReviewRepository.Setup(r => r.GetByStatusAsync(ReviewStatus.UnderReview)).Returns(
                Task.FromResult(new List<VacancyReview>
                {
                    CreateVacancyReview("cd87db96-fe65-4d1b-9070-e50fb74a647a", "user A", ReviewStatus.UnderReview, "2018-08-20T13:59:59Z", "2018-08-19T18:00:00Z"),
                    CreateVacancyReview("92a9b68b-4555-425d-92a1-5d8ea625cf5b", "user A", ReviewStatus.UnderReview, "2018-08-20T14:00:00Z", "2018-08-19T18:32:00Z"),
                    CreateVacancyReview("7a636aad-a3a5-4293-bae6-5c5a8b199265", "user A", ReviewStatus.UnderReview, "2018-08-20T14:00:01Z", "2018-08-19T18:45:00Z"),

                    CreateVacancyReview("c7267018-c54d-4aa4-a59d-bf4a46fa24be", "user B", ReviewStatus.UnderReview, "2018-08-20T13:59:59Z", "2018-08-18T12:00:00Z"),
                    CreateVacancyReview("46cf4c11-6129-4570-805a-b18e939f7175", "user B", ReviewStatus.UnderReview, "2018-08-20T13:59:59Z", "2018-08-18T12:00:01Z")
                }));

            vacancyReviewRepository.Setup(r => r.GetByStatusAsync(ReviewStatus.PendingReview)).Returns(
                Task.FromResult(new List<VacancyReview>
                {
                    CreateVacancyReview("956a5556-0703-4aa2-bee9-9035a7e07516", null, ReviewStatus.PendingReview, null, "2018-08-20T18:00:01Z"),
                    CreateVacancyReview("8353063d-bc67-4588-83d9-2281080f8112", null, ReviewStatus.PendingReview, null, "2018-08-20T18:00:00Z")
                }));

            var config = Options.Create(new NextVacancyReviewServiceConfiguration
            {
                VacancyReviewAssignationTimeoutMinutes = 180
            });
            
            var sut = new NextVacancyReviewService(config, timeProvider.Object, vacancyReviewRepository.Object);

            var nextVacancyReview = sut.GetNextVacancyReviewAsync(userId).Result;

            Assert.Equal(Guid.Parse(expectedReviewId), nextVacancyReview.Id);
        }

        private static VacancyReview CreateVacancyReview(string id, string userId, ReviewStatus status, string reviewedDate, string createdDate)
        {
            return new VacancyReview
            {
                Id = Guid.Parse(id),
                ReviewedDate = reviewedDate != null ? DateTime.Parse(reviewedDate) : (DateTime?)null,
                CreatedDate = DateTime.Parse(createdDate),
                Status = status,
                ReviewedByUser = userId != null ? new VacancyUser {UserId = userId} : null
            };
        }
    }
}
