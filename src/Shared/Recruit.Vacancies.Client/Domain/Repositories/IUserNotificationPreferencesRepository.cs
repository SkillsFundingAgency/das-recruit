using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IUserNotificationPreferencesRepository
    {
        Task<UserNotificationPreferences> GetAsync(Guid userId);
        Task UpsertAsync(UserNotificationPreferences preferences);
    }
}