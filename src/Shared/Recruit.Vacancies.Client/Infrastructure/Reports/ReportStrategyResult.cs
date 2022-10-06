using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Reports
{
    public class ReportStrategyResult
    {
        public IEnumerable<KeyValuePair<string, string>> Headers { get; set; }
        public string Data { get; set; }
        public string Query { get; set; }

        public ReportStrategyResult(IEnumerable<KeyValuePair<string, string>> headers, string data, string query)
        {
            Headers = headers;
            Data = data;
            Query = query;
        }
    }
}
