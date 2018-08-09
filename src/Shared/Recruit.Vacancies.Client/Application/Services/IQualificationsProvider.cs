using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IQualificationsProvider
    {
        Task<IList<string>> GetQualificationsAsync();
    }
}