using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class SubmitVacancyCommand : ICommand, IRequest
    {
        public Guid VacancyId { get;set; }
        public VacancyUser User { get; set; }
        public string EmployerDescription { get; set; }
    }
}
