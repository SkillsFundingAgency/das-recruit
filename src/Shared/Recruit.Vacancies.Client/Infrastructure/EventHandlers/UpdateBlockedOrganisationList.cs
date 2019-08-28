using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateBlockedOrganisationList : INotificationHandler<ProviderBlockedEvent>
    {
        private readonly IBlockedOrganisationsProjectionService _projectionService;
        public UpdateBlockedOrganisationList(IBlockedOrganisationsProjectionService projectionService)
        {
            _projectionService = projectionService;
        }
        public Task Handle(ProviderBlockedEvent notification, CancellationToken cancellationToken)
        {
            return _projectionService.RebuildAllBlockedOrganisationsAsync();
        }
    }
}