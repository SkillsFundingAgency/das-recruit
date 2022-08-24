using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class ReferVacancyReviewCommand : ICommand, IRequest<Unit>
    {
        public Guid ReviewId { get; }
        public string ManualQaComment { get; }
        public List<ManualQaFieldIndicator> ManualQaFieldIndicators { get; }
        public List<Guid> SelectedAutomatedQaRuleOutcomeIds { get; }

        public ReferVacancyReviewCommand(Guid reviewId, string manualQaComment, List<ManualQaFieldIndicator> manualQaFieldIndicators, List<Guid> selectedAutomatedQaRuleOutcomeIds)
        {
            ReviewId = reviewId;
            ManualQaComment = manualQaComment;
            ManualQaFieldIndicators = manualQaFieldIndicators;
            SelectedAutomatedQaRuleOutcomeIds = selectedAutomatedQaRuleOutcomeIds;
        }
    }
}
