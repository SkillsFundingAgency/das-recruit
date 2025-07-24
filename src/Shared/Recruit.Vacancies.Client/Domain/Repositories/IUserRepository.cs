using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories;

public interface IUserRepository
{
    Task<User> GetAsync(string idamsUserId);
    Task<User> GetByDfEUserId(string dfEUserId);
    Task UpsertUserAsync(User user);
    Task<List<User>> GetEmployerUsersAsync(string accountId);
    Task<List<User>> GetProviderUsersAsync(long ukprn);
    Task<User> GetUserByEmail(string email, UserType userType);
}

public interface IUserWriteRepository
{
    Task UpsertUserAsync(User user);
}