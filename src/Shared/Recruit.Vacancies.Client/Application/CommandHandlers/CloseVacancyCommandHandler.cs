using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CloseVacancyCommandHandler : IRequestHandler<CloseVacancyCommand>
    {
        private readonly IVacancyService _vacancyService;

        public CloseVacancyCommandHandler(IVacancyService vacancyService)
        {
            _vacancyService = vacancyService;
        }

        public Task Handle(CloseVacancyCommand message, CancellationToken cancellationToken)
        {
            return _vacancyService.CloseVacancyImmediately(message.VacancyId, message.User, message.ClosureReason);
        }
    }
}
