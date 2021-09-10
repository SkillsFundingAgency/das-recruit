using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UserSignedInCommand : ICommand, IRequest<Unit>
    {
        public VacancyUser User { get; private set; }
        public UserType UserType { get; private set; }

        public UserSignedInCommand(VacancyUser vacancyUser, UserType userType)
        {
            User = vacancyUser;
            UserType = userType;
        }
    }
}