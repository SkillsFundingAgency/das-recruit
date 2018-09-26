using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IGetVacancyTitlesProvider
    {
        Task<IList<string>> GetVacancyTitlesAsync(string larsCode);
    }
}