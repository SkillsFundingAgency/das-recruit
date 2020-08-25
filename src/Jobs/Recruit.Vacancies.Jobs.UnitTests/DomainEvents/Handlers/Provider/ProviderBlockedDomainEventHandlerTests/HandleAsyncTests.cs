using System;
using System.Collections.Generic;
using System.Linq;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Jobs.UnitTests.DomainEvents.Handlers.Provider.ProviderBlockedDomainEventHandlerTests;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Recruit.Vacancies.Jobs.UnitTests.DomainEvents.Handlers.Provider.ProviderBlockedDomainEventHandlerTests
{
    public class HandleAsyncTests : ProviderBlockedDomainEventHandlerTestBase
    {
        private string _employerAccount1 = "EmployerAccount1";
        private string _employerAccount2 = "EmployerAccount2";
        private string _employerAccount3 = "EmployerAccount3";
        private string _legalEntity1 = "LegalEntity1";
        private string _legalEntity2 = "LegalEntity2";
        private long _ukprn = 1234;
        private readonly IEnumerable<ProviderVacancySummary> vacancies;
        private readonly DateTime _blockedDate = DateTime.Now;

        public HandleAsyncTests()
        {
            var data = new ProviderBlockedEvent()
            {
                Ukprn = _ukprn,
                ProviderName = "provider name",
                BlockedDate = _blockedDate,
                QaVacancyUser = new VacancyUser()
            };

            vacancies = GetVacancies(OwnerType.Provider, _employerAccount1, 1, VacancyStatus.Live)
                .Concat(GetVacancies(OwnerType.Employer, _employerAccount2, 1, VacancyStatus.Live));
            var info = GetProviderProfile();
            var sut = GetSut(info, vacancies);
            var payload = JsonConvert.SerializeObject(data);
            sut.HandleAsync(payload).Wait();
        }

        [Fact]
        public void ShouldNotifyEmployersAboutRevokedPermission()
        {            
            MockMessaging.Verify(q => q.PublishEvent(It.Is<ProviderBlockedOnLegalEntityEvent>(p => 
                p.Ukprn == _ukprn && 
                p.EmployerAccountId == _employerAccount1 && 
                p.AccountLegalEntityPublicHashedId == _legalEntity1
            )));

            MockMessaging.Verify(q => q.PublishEvent(It.Is<ProviderBlockedOnLegalEntityEvent>(p => 
                p.Ukprn == _ukprn && 
                p.EmployerAccountId == _employerAccount1 && 
                p.AccountLegalEntityPublicHashedId == _legalEntity2
            )));

            MockMessaging.Verify(q => q.PublishEvent(It.IsAny<ProviderBlockedOnLegalEntityEvent>()), Times.Exactly(2));
        }

        [Fact]
        public void ShouldRaiseEventsToUpdateVacancies()
        {
            MockMessaging.Verify(m => m.PublishEvent(It.Is<ProviderBlockedOnVacancyEvent>(v => 
                v.Ukprn == _ukprn && 
                v.VacancyId == vacancies.First().Id && 
                v.BlockedDate == _blockedDate
            )));

            MockMessaging.Verify(m => m.PublishEvent(It.Is<ProviderBlockedOnVacancyEvent>(v => 
                v.Ukprn == _ukprn && 
                v.VacancyId == vacancies.Last().Id && 
                v.BlockedDate == _blockedDate
            )));

            MockMessaging.Verify(m => m.PublishEvent(It.IsAny<ProviderBlockedOnVacancyEvent>()), Times.Exactly(2));
        }

        [Fact]
        public void ShouldNotifyProvider()
        {
            MockCommunicationQueueService.Verify(c => c.AddMessageAsync(It.Is<CommunicationRequest>(r => 
                r.RequestType == CommunicationConstants.RequestType.ProviderBlockedProviderNotification && 
                r.ParticipantsResolverName == CommunicationConstants.ParticipantResolverNames.ProviderParticipantsResolverName && 
                r.Entities.Count == 2 && 
                r.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Provider && (long)e.EntityId == _ukprn) && 
                r.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.ApprenticeshipServiceConfig && e.EntityId == null)
            )));
            
            MockCommunicationQueueService.Verify(c => c.AddMessageAsync(It.Is<CommunicationRequest>(r => 
                r.RequestType == CommunicationConstants.RequestType.ProviderBlockedProviderNotification)), Times.Once);
        }

        [Fact]
        public void ShouldNotifyEmployerAboutTransfer()
        {
            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForTransferredVacancies
            )), Times.Once);

            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForTransferredVacancies &&
                c.ParticipantsResolverName == CommunicationConstants.ParticipantResolverNames.EmployerParticipantsResolverName && 
                c.TemplateProviderName == CommunicationConstants.ServiceName && 
                c.DataItems.Any(d => d.Key == CommunicationConstants.DataItemKeys.Employer.VacanciesTransferredCountText && d.Value.StartsWith('1')) && 
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Employer && e.EntityId.ToString() == _employerAccount1) &&
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Provider && (long)e.EntityId == _ukprn)
                )));
        }

        [Fact]
        public void ShouldNotifyEmployerAboutLiveVacancies()
        {
            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForLiveVacancies
            )), Times.Once);

            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForLiveVacancies &&
                c.ParticipantsResolverName == CommunicationConstants.ParticipantResolverNames.EmployerParticipantsResolverName && 
                c.TemplateProviderName == CommunicationConstants.ServiceName && 
                c.DataItems.Count() == 0 && 
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Employer && e.EntityId.ToString() == _employerAccount2) &&
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Provider && (long)e.EntityId == _ukprn) &&
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.ApprenticeshipServiceConfig && e.EntityId == null)
            )));
        }

        [Fact]
        public void ShouldNotifyEmployerAboutRevokedPermissions()
        {
            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForPermissionOnly
            )), Times.Once);

            MockCommunicationQueueService.Verify(q => q.AddMessageAsync(It.Is<CommunicationRequest>(c => 
                c.RequestType == CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForPermissionOnly &&
                c.ParticipantsResolverName == CommunicationConstants.ParticipantResolverNames.EmployerParticipantsResolverName && 
                c.TemplateProviderName == CommunicationConstants.ServiceName && 
                c.DataItems.Count() == 0 && 
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Employer && e.EntityId.ToString() == _employerAccount3) &&
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.Provider && (long)e.EntityId == _ukprn) &&
                c.Entities.Any(e => e.EntityType == CommunicationConstants.EntityTypes.ApprenticeshipServiceConfig && e.EntityId == null)
            )));
        }

        protected ProviderEditVacancyInfo GetProviderProfile()
        {
            return new ProviderEditVacancyInfo()
            {
                Employers = new EmployerInfo[] 
                { 
                    new EmployerInfo 
                    {
                        EmployerAccountId = _employerAccount1,
                        LegalEntities = new List<LegalEntity>() 
                        {
                            new LegalEntity() { AccountLegalEntityPublicHashedId = _legalEntity1 },
                            new LegalEntity() { AccountLegalEntityPublicHashedId = _legalEntity2 }
                        }
                    },
                    new EmployerInfo { EmployerAccountId = _employerAccount3 } 
                }
            };
        }
    }
}