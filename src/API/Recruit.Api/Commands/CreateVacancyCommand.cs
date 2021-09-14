using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MediatR;

namespace SFA.DAS.Recruit.Api.Commands
{
    public class CreateVacancyCommand : IRequest<CreateVacancyCommandResponse>
    {
        public Vacancy Vacancy { get ; set ; }
        public VacancyUser CreatedByUser { get ; set ; }
        public long Ukprn { get ; set ; }
    }
}