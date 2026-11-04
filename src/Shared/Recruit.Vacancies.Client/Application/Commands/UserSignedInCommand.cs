using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UserSignedInCommand(VacancyUser vacancyUser, UserType userType) : ICommand, IRequest<Unit>
    {
        public VacancyUser User { get; private set; } = vacancyUser;
        public UserType UserType { get; private set; } = userType;
    }
}