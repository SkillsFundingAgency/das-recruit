using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.QueryStore;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Handlers
{
    public class UpdateApprenticeshipProgrammesCommandHandler: IRequestHandler<UpdateApprenticeshipProgrammesCommand>
    {
        private readonly IQueryStoreWriter _queryStore;

        public UpdateApprenticeshipProgrammesCommandHandler(IQueryStoreWriter queryStore)
        {
            _queryStore = queryStore;
        }

        public async Task Handle(UpdateApprenticeshipProgrammesCommand message, CancellationToken cancellationToken)
        {
            await _queryStore.UpdateApprenticeshipProgrammesAsync(message.ApprenticeshipProgrammes);
        }
    }
}
