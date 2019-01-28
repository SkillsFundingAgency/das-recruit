using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class CreateProviderOwnedVacancyCommand : ICommand, IRequest
    {
        public Guid VacancyId { get; set; }
        public SourceOrigin Origin { get; set; }
        public long Ukprn { get; set; }
        public string EmployerAccountId { get; set; }
        public VacancyUser User { get; set; }
        public UserType UserType { get; set; }
    }
}
