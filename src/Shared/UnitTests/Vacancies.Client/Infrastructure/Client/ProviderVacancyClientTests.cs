using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client
{
    public class ProviderVacancyClientTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_Parameters_Are_Passed_To_The_ApplicationReviewsToUnsuccessful_Command_And_Handled(
            Guid vacancyId,
            [Frozen] Mock<IMessaging> messaging, 
            VacancyClient client)
        {
            var applicationReviewsToUnsuccessful = new List<VacancyApplication>
            {
                new() { ApplicationReviewId = Guid.NewGuid() },
                new() { ApplicationReviewId = Guid.NewGuid() }
            };

            var candidateFeedback = "Some candidate feedback";
            var user = new VacancyUser();

            await client.SetApplicationReviewsToUnsuccessful(applicationReviewsToUnsuccessful.Select(c=>c.ApplicationReviewId), candidateFeedback, user, vacancyId);

            messaging.Verify(x => x.SendCommandAsync(It.Is<ApplicationReviewsUnsuccessfulCommand>(c =>
                c.CandidateFeedback.Equals(candidateFeedback) &&
                c.ApplicationReviews.Count().Equals(applicationReviewsToUnsuccessful.Select(x=>x.ApplicationReviewId).ToList().Count) &&
                c.VacancyId.Equals(vacancyId)
            )));

        }
    }
}