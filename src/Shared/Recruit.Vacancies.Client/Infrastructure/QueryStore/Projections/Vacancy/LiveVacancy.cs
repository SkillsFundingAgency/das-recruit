namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy
{
    public class LiveVacancy : VacancyProjectionBase
    {
        public LiveVacancy() : base(QueryViewType.LiveVacancy.TypeName)
        {
        }
    }
}
