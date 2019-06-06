namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA
{
    // This message class is a duplicate of https://github.com/SkillsFundingAgency/das-vacancyservicesshared/blob/master/src/SFA.Apprenticeships.Core/Entities/Domain/Vacancies/VacancyStatuses.cs
    // Ensure you keep both classes in sync.
    public enum FaaVacancyStatuses
    {
        Unknown = 0,
        // Current vacancy which can be applied for.
        Live = 1,
        // Vacancy which can no longer be applied for.
        Unavailable = 2,
        // Vacancy which has expired but can still be viewed.
        Expired = 3
    }
}
