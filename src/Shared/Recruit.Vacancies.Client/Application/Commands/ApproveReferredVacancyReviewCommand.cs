using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class ApproveReferredVacancyReviewCommand : CommandBase, ICommand, IRequest
    {
        public Guid ReviewId { get; set; }
        public string ShortDescription { get; set; }
        public string VacancyDescription { get; set; }
        public string TrainingDescription { get; set; }
        public string OutcomeDescription { get; set; }
        public string ThingsToConsider { get; set; }
        public string EmployerDescription { get; set; }
    }
}
