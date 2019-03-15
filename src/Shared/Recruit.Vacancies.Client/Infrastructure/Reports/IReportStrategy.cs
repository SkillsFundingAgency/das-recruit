using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Reports
{
    public interface IReportStrategy
    {
        Task<string> GetReportDataAsync(Dictionary<string, object> parameters);
    }
}
