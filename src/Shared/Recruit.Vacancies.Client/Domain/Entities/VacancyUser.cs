namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class VacancyUser
    {
        public string UserId { get; set; }
        public string DfEUserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public long? Ukprn { get; set; }
    }
}
