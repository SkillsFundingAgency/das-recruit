using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Providers
{
    public interface IGetVacancyTitlesProvider
    {
        Task<IList<string>> GetVacancyTitlesAsync(string larsCode);
    }
}