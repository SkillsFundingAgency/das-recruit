using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public static class VacancyTitlePopularityRuleMessageTemplate
    {
        public static string ToText(TitlePopularityData msgData, string fieldName)
        {
            return $"The {fieldName.ToLower()} is not common for apprenticeships with the training {msgData.TrainingType} {msgData.TrainingCode} - {msgData.TrainingTitle}.";
        }
    }
}