namespace Esfa.Recruit.Provider.Web.Configuration
{
    public class ServiceParameters
    {
        public virtual VacancyType? VacancyType { get; set; }
    }

    public enum VacancyType
    {
        Apprenticeship,
        Traineeship
    }
}