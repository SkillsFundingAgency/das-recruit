using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Provider;
using Microsoft.Extensions.Logging;
using Moq;

namespace Esfa.Recruit.Vacancies.Jobs.UnitTests.DomainEvents.Handlers.Provider.ProviderBlockedDomainEventHandlerTests
{
    public abstract class ProviderBlockedDomainEventHandlerTestBase
    {
        protected readonly Mock<ICommunicationQueueService> MockCommunicationQueueService = new Mock<ICommunicationQueueService>();
        protected readonly Mock<IMessaging> MockMessaging = new Mock<IMessaging>();

        protected ProviderBlockedDomainEventHandler GetSut(ProviderEditVacancyInfo providerInfo, IEnumerable<ProviderVacancySummary> vacancies)
        {
            var mockQueryStoreReader = new Mock<IQueryStoreReader>();
            mockQueryStoreReader.Setup(q => q.GetProviderVacancyDataAsync(It.IsAny<long>())).ReturnsAsync(providerInfo);

            var mockVacancyQuery = new Mock<IVacancyQuery>();
            mockVacancyQuery.Setup(v => v.GetVacanciesAssociatedToProvider(It.IsAny<long>())).ReturnsAsync(vacancies);

            var mockLogger = new Mock<ILogger<ProviderBlockedDomainEventHandler>>();
            return new ProviderBlockedDomainEventHandler(
                mockQueryStoreReader.Object, mockVacancyQuery.Object, MockMessaging.Object, 
                MockCommunicationQueueService.Object, mockLogger.Object);
        }

        protected ProviderEditVacancyInfo GetEmptyProviderProfile(params string[] employerAccountIds)
        {
            return new ProviderEditVacancyInfo()
            {
                Employers = employerAccountIds.Select(e => new EmployerInfo { EmployerAccountId = e })
            };
        }

        private int referenceNumber = 1000;
        protected List<ProviderVacancySummary> GetVacancies(OwnerType owner, string employerId, int count, VacancyStatus status)
        {
            var vacancies = new List<ProviderVacancySummary>();
            for(var i = 1; i <= count; i++)
            {
                vacancies.Add(
                    new ProviderVacancySummary 
                    {
                        Id = Guid.NewGuid(),
                        VacancyOwner = owner,
                        EmployerAccountId = employerId,
                        VacancyReference = ++referenceNumber,
                        Status = status
                    }
                );
            }
            return vacancies;
        }
    }
}