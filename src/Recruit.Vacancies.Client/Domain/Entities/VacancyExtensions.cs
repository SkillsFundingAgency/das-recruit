using Esfa.Recruit.Storage.Client.Domain.Enum;

namespace Esfa.Recruit.Storage.Client.Domain.Entities
{ 
    public static class VacancyExtensions
    {
        public static bool IsSubmittable(this Vacancy vacancy)
        {
            return vacancy.Status == VacancyStatus.Draft;
        }
    }
}
