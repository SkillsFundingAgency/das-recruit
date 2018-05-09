using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateVacancyReviewCommandHandler: IRequestHandler<UpdateVacancyReviewCommand>
    {
        private readonly IVacancyReviewRepository _vacancyReviewRepository;

        public UpdateVacancyReviewCommandHandler(IVacancyReviewRepository vacancyReviewRepository)
        {
            _vacancyReviewRepository = vacancyReviewRepository;
        }

        public Task Handle(UpdateVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            return _vacancyReviewRepository.UpdateAsync(message.Review);
        }
    }
}
