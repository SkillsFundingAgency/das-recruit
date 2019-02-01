using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class SubmitVacancyCommand : ICommand, IRequest
    {
        public SubmitVacancyCommand(Guid vacancyId, VacancyUser user)
        {
            VacancyId = vacancyId;
            User = user;
        }

        public SubmitVacancyCommand(Guid vacancyId, VacancyUser user, string employerDescription)
        {
            VacancyId = vacancyId;
            User = user;
            EmployerDescription = employerDescription;
        }

        public Guid VacancyId { get;}
        public VacancyUser User { get; }
        public string EmployerDescription { get; }
    }
}
