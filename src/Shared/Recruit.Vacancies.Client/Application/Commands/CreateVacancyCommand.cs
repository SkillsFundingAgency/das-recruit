using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class CreateVacancyCommand : ICommand, IRequest
    {
        public Guid VacancyId { get; set; }
        public SourceOrigin Origin { get; set; }
        public string Title { get;set; }
        public int NumberOfPositions { get; set; }
        public string EmployerAccountId { get; set; }
        public VacancyUser User { get; set; }
    }
}
