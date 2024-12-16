using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using FluentAssertions;
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
            [Frozen] Mock<IMessaging> messaging,
            VacancyClient client)
        {
            await client.CreateProviderApplicationsReportAsync(ukprn, fromDate, toDate, user, reportName);

            messaging.Verify(x => x.SendCommandAsync(It.Is<CreateReportCommand>(c =>
                c.ReportId != Guid.Empty &&
                c.Owner.Ukprn.Equals(ukprn) &&
                c.Owner.OwnerType.Equals(ReportOwnerType.Provider) &&
                c.Parameters["VacancyType"].Equals(VacancyType.Apprenticeship.ToString()) &&
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

            messaging.Verify(x => x.SendCommandAsync(It.Is<ApplicationReviewsUnsuccessfulCommand>(c =>
                c.CandidateFeedback.Equals(candidateFeedback) &&
                c.ApplicationReviews.Equals(applicationReviewsToUnsuccessful)
            )));

        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Getting_UserNotificationPreferences_And_DfESignInId_Not_Passed_And_Preferences_Dont_Exist_Then_Returned_With_Default(
            string idamsUserId,
            [Frozen] Mock<IUserNotificationPreferencesRepository> userNotificationPreferencesRepository,
            VacancyClient client)
        {
            userNotificationPreferencesRepository.Setup(x => x.GetAsync(idamsUserId))
                .ReturnsAsync((UserNotificationPreferences)null);
            
            var actual = await client.GetUserNotificationPreferencesAsync(idamsUserId);

            actual.DfeUserId.Should().BeNull();
            actual.Id.Should().Be(idamsUserId);
            actual.NotificationFrequency.Should().BeNull();
            actual.NotificationScope.Should().BeNull();
        }

        [Test, MoqAutoData]
        public async Task Then_If_Getting_UserNotificationPreferences_And_DfESignInId_Passed_And_Preferences_Dont_Exist_Then_Null_Returned_With_Default(
                string dfESignInId,
                string idamsUserId,
                [Frozen] Mock<IUserNotificationPreferencesRepository> userNotificationPreferencesRepository,
                VacancyClient client)
        {
            userNotificationPreferencesRepository.Setup(x => x.GetAsync(idamsUserId))
                .ReturnsAsync((UserNotificationPreferences)null);
            userNotificationPreferencesRepository.Setup(x => x.GetByDfeUserId(dfESignInId))
                .ReturnsAsync((UserNotificationPreferences)null);
            
            var actual = await client.GetUserNotificationPreferencesByDfEUserIdAsync(idamsUserId, dfESignInId);

            actual.DfeUserId.Should().Be(dfESignInId);
            actual.Id.Should().Be(idamsUserId);
            actual.NotificationFrequency.Should().BeNull();
            actual.NotificationScope.Should().BeNull();
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Getting_UserNotificationPreferences_And_DfESignInId_Passed_And_Preferences_Exist_Then_Returned(
                string dfESignInId,
                string idamsUserId,
                UserNotificationPreferences preference1,
                UserNotificationPreferences preference2,
                [Frozen] Mock<IUserNotificationPreferencesRepository> userNotificationPreferencesRepository,
                VacancyClient client)
        {
            userNotificationPreferencesRepository.Setup(x => x.GetAsync(idamsUserId))
                .ReturnsAsync(preference1);
            userNotificationPreferencesRepository.Setup(x => x.GetByDfeUserId(dfESignInId))
                .ReturnsAsync(preference2);
            
            var actual = await client.GetUserNotificationPreferencesByDfEUserIdAsync(idamsUserId, dfESignInId);

            actual.Should().BeEquivalentTo(preference2);
        }
    }
}