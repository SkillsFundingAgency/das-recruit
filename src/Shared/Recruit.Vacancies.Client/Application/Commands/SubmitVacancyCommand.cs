using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class SubmitVacancyCommand : ICommand, IRequest
    {
        public SubmitVacancyCommand(Guid vacancyId, VacancyUser user, OwnerType submittedFrom)
        {
            VacancyId = vacancyId;
            User = user;
            SubmittedFrom = submittedFrom;
        }

        public SubmitVacancyCommand(Guid vacancyId, VacancyUser user, string employerDescription, OwnerType submittedFrom)
        {
            VacancyId = vacancyId;
            User = user;
            EmployerDescription = employerDescription;
            SubmittedFrom = submittedFrom;
        }

        public Guid VacancyId { get;}
        public VacancyUser User { get; }
        public string EmployerDescription { get; }
        public OwnerType SubmittedFrom { get; }
    }
}
