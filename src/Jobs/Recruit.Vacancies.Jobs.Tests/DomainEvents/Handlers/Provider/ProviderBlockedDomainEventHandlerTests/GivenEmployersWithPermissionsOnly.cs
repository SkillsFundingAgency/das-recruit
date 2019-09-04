using System;
using System.Linq;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Esfa.Recruit.Vacancies.Jobs.Tests.DomainEvents.Handlers.Provider.ProviderBlockedDomainEventHandlerTests
{
    public class GivenEmployersWithPermissionsOnly : ProviderBlockedDomainEventHandlerTestBase
    {
        [Fact]
        public async Task ShouldNotifyEmployersAboutRevokedPermission()
        {
            var employerAccount1 = "EmployerAccount1";
            var employerAccount2 = "EmployerAccount2";
            var employerAccount3 = "EmployerAccount3";

            var ukprn = 1234;
            var data = new ProviderBlockedEvent()
            {
                Ukprn = ukprn,
                ProviderName = "provider name",
                BlockedDate = DateTime.Now,
                QaVacancyUser = new VacancyUser()
            };

            var vacancies = GetVacancies(OwnerType.Employer, employerAccount1, 1, VacancyStatus.Live);
            var info = GetEmptyProviderProfile(employerAccount1, employerAccount2, employerAccount3);
            var sut = GetSut(info, vacancies);
            var payload = JsonConvert.SerializeObject(data);
            await sut.HandleAsync(payload);

            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForPermissionOnly
                )), Times.Exactly(2));

            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForPermissionOnly &&
                c.ParticipantsResolverName == CommunicationConstants.ParticipantResolverNames.EmployerParticipantsResolverName && 
                c.TemplateProviderName == CommunicationConstants.ServiceName && 
                c.DataItems.Count == 0 && 
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Employer && e.EntityId.ToString() == employerAccount2) &&
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Provider && (long)e.EntityId == ukprn)
                )));

            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForPermissionOnly &&
                c.ParticipantsResolverName == CommunicationConstants.ParticipantResolverNames.EmployerParticipantsResolverName && 
                c.TemplateProviderName == CommunicationConstants.ServiceName && 
                c.DataItems.Count == 0 && 
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Employer && e.EntityId.ToString() == employerAccount3) &&
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Provider && (long)e.EntityId == ukprn)
                )));

            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForPermissionOnly &&
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Employer && e.EntityId.ToString() == employerAccount1)
                )), Times.Never);
        }
    }
}