using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IApplicationReviewQuery
    {
        Task<List<T>> GetForEmployerAsync<T>(string employerAccountId);
        Task<List<T>> GetForVacancyAsync<T>(long vacancyReference);
    }
}
