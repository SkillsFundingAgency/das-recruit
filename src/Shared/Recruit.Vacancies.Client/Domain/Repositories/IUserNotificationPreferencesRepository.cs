using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IUserNotificationPreferencesRepository
    {
        Task<UserNotificationPreferences> GetAsync(string idamsUserId);
        Task UpsertAsync(UserNotificationPreferences preferences);
        Task<UserNotificationPreferences> GetByDfeUserId(string dfeUserId);
    }
}