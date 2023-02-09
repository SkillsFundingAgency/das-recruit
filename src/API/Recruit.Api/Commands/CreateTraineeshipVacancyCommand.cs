using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MediatR;

namespace SFA.DAS.Recruit.Api.Commands
{
    public class CreateTraineeshipVacancyCommand : IRequest<CreateTraineeshipVacancyCommandResponse>
    {
        public Vacancy Vacancy { get; set; }
        public VacancyUser VacancyUserDetails { get; set; }
        public bool ValidateOnly { get; set; }
    }
}