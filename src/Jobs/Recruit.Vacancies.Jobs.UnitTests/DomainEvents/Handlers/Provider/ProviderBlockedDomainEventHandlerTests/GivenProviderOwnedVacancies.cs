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

namespace Esfa.Recruit.Vacancies.Jobs.UnitTests.DomainEvents.Handlers.Provider.ProviderBlockedDomainEventHandlerTests
{
    public class GivenProviderOwnedVacancies : ProviderBlockedDomainEventHandlerTestBase
    {
        [Fact]
        public async Task MustNotifyEmployersWithTransferCount()
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

            var payload = JsonConvert.SerializeObject(data);
            var vacancies = GetVacancies(OwnerType.Provider, employerAccount1, 2, VacancyStatus.Live)
                .Concat(GetVacancies(OwnerType.Provider, employerAccount2, 1, VacancyStatus.Draft))
                .Concat(GetVacancies(OwnerType.Employer, employerAccount3, 1, VacancyStatus.Draft));
                
            var sut = GetSut(GetEmptyProviderProfile(), vacancies);
            await sut.HandleAsync(payload);

            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForTransferredVacancies
                )), Times.Exactly(2));


            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForTransferredVacancies &&
                c.ParticipantsResolverName == CommunicationConstants.ParticipantResolverNames.EmployerParticipantsResolverName && 
                c.TemplateProviderName == CommunicationConstants.ServiceName && 
                c.DataItems.Any(d => d.Key == CommunicationConstants.DataItemKeys.Employer.VacanciesTransferredCount && d.Value == "2") && 
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Employer && e.EntityId.ToString() == employerAccount1) &&
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Provider && (long)e.EntityId == ukprn)
                )));

            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForTransferredVacancies &&
                c.ParticipantsResolverName == CommunicationConstants.ParticipantResolverNames.EmployerParticipantsResolverName && 
                c.TemplateProviderName == CommunicationConstants.ServiceName && 
                c.DataItems.Any(d => d.Key == CommunicationConstants.DataItemKeys.Employer.VacanciesTransferredCount && d.Value == "1") && 
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Employer && e.EntityId.ToString() == employerAccount2) &&
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Provider && (long)e.EntityId == ukprn)
                )));
        }
    }
}