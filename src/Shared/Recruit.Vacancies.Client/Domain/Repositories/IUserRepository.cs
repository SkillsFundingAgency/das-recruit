using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetAsync(string idamsUserId);
        Task UpsertUserAsync(User user);
    }
}
