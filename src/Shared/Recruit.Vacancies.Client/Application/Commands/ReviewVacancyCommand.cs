using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class ReviewVacancyCommand(
        Guid vacancyId,
        VacancyUser user,
        OwnerType submissionOwner,
        string employerDescription = null)
        : ICommand, IRequest<Unit>
    {
        public Guid VacancyId { get;} = vacancyId;
        public VacancyUser User { get; } = user;
        public string EmployerDescription { get; } = employerDescription;
        public OwnerType SubmissionOwner { get; } = submissionOwner;
    }
}
