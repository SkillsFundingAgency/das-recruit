using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client
{
    public class ProviderVacancyClientTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_Parameters_Are_Passed_To_The_Command_And_Handled(
            long ukprn,
            DateTime fromDate,
            DateTime toDate,
            VacancyUser user,
            string reportName,
            VacancyType vacancyType,
            [Frozen] Mock<IMessaging> messaging,
            VacancyClient client)
        {
            await client.CreateProviderApplicationsReportAsync(ukprn, fromDate, toDate, user, reportName, vacancyType);

            messaging.Verify(x => x.SendCommandAsync(It.Is<CreateReportCommand>(c =>
                c.ReportId != Guid.Empty &&
                c.Owner.Ukprn.Equals(ukprn) &&
                c.Owner.OwnerType.Equals(ReportOwnerType.Provider) &&
                c.Parameters["VacancyType"].Equals(vacancyType.ToString()) &&
                c.Parameters["Ukprn"].Equals(ukprn)
                )));
        }

        
        [Test, MoqAutoData]
        public async Task Then_The_Parameters_Are_Passed_To_The_ApplicationReviewsToUnsuccessful_Command_And_Handled([Frozen] Mock<IMessaging> messaging, VacancyClient client)
        {
            var applicationReviewsToUnsuccessful = new List<VacancyApplication>
            {
                new VacancyApplication { ApplicationReviewId = Guid.NewGuid() },
                new VacancyApplication { ApplicationReviewId = Guid.NewGuid() }
            };

            var candidateFeedback = "Some candidate feedback";
            var user = new VacancyUser();

            await client.SetApplicationReviewsToUnsuccessful(applicationReviewsToUnsuccessful, candidateFeedback, user);

            messaging.Verify(x => x.SendCommandAsync(It.Is<ApplicationReviewsToUnsuccessfulCommand>(c =>
                c.CandidateFeedback.Equals(candidateFeedback) &&
                c.ApplicationReviews.Equals(applicationReviewsToUnsuccessful)
            )));

        }
    }
}