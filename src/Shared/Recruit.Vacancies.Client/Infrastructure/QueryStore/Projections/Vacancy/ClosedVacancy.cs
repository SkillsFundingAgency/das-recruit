namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy
{
    public class ClosedVacancy : VacancyProjectionBase
    {
        public ClosedVacancy() : base(QueryViewType.ClosedVacancy.TypeName)
        {}
    }
}
