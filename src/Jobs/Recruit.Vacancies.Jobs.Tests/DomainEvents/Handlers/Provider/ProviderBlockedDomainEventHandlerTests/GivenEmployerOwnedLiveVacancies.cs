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
    public class GivenEmployerOwnedLiveVacancies : ProviderBlockedDomainEventHandlerTestBase
    {
        [Fact]
        public async Task ShouldNotifyEmployersAboutLiveVacancy()
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

            var vacancies = GetVacancies(OwnerType.Employer, employerAccount1, 1, VacancyStatus.Live)
                .Concat(GetVacancies(OwnerType.Employer, employerAccount2, 1, VacancyStatus.Live))
                .Concat(GetVacancies(OwnerType.Employer, employerAccount3, 1, VacancyStatus.Draft));

            var sut = GetSut(GetEmptyProviderProfile(employerAccount1), vacancies);
            var payload = JsonConvert.SerializeObject(data);
            await sut.HandleAsync(payload);
            
            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForLiveVacancies 
                )), Times.Exactly(2));

            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForLiveVacancies &&
                c.ParticipantsResolverName == CommunicationConstants.ParticipantResolverNames.EmployerParticipantsResolverName && 
                c.TemplateProviderName == CommunicationConstants.ServiceName && 
                c.DataItems.Count == 0 && 
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Employer && e.EntityId.ToString() == employerAccount1) &&
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Provider && (long)e.EntityId == ukprn)
                )));

            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForLiveVacancies &&
                c.ParticipantsResolverName == CommunicationConstants.ParticipantResolverNames.EmployerParticipantsResolverName && 
                c.TemplateProviderName == CommunicationConstants.ServiceName && 
                c.DataItems.Count == 0 && 
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Employer && e.EntityId.ToString() == employerAccount2) &&
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Provider && (long)e.EntityId == ukprn)
                )));
        }
    }
}