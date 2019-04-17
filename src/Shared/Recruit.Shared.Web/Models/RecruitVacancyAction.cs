using System;

namespace Esfa.Recruit.Shared.Web.Models
{
    public class RecruitVacancyAction
    {
        public VacancyActionType ActionType { get; private set; }
        public Guid? VacancyId { get; private set; }

        public RecruitVacancyAction(VacancyActionType actionType, Guid? vacancyId)
        {
            ActionType = actionType;
            VacancyId = vacancyId;
        }
    }
}