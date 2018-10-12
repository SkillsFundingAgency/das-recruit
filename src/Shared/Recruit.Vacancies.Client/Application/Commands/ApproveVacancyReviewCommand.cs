using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class ApproveVacancyReviewCommand : ICommand, IRequest
    {
        public Guid ReviewId { get; internal set; }
        public string ManualQaComment { get; internal set; }
        public List<ManualQaFieldIndicator> ManualQaFieldIndicators { get; internal set; }
        public List<Guid> SelectedAutomatedQaRuleOutcomeIds { get; internal set; }
    }
}
