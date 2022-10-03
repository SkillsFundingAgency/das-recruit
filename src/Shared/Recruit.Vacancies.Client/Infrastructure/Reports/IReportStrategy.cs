using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Reports
{
    public interface IReportStrategy
    {
        Task<ReportStrategyResult> GetReportDataAsync(Dictionary<string, object> parameters);
        ReportDataType ResolveFormat(string fieldName);

        Task<string> GetApplicationReviewsRecursiveAsync(string queryJson);
    }
}
