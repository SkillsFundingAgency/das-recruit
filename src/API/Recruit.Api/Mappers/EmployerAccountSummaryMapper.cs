using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Services;

namespace SFA.DAS.Recruit.Api.Mappers
{
    public static class EmployerAccountSummaryMapper
    {
        public static EmployerAccountSummary MapFromEmployerDashboard(EmployerDashboard dashboard, string employerAccountId)
        {
            var vacancies = dashboard.Vacancies.ToList();

            return new EmployerAccountSummary
            {
                EmployerAccountId = employerAccountId,
                TotalNoOfVacancies = vacancies.Count(),
                TotalVacancyStatusCounts = GetTotalVacancyStatusCounts(vacancies),
                LegalEntityVacancySummaries = GetLegalEntities(vacancies)
            };
        }

        private static IDictionary<string, int> GetTotalVacancyStatusCounts(IEnumerable<VacancySummaryProjection> vacancies)
        {
            return vacancies.GroupBy(v => v.Status)
                            .ToDictionary(
                                            statusGrp => statusGrp.Key.ToString().ToLower(),
                                            statusGrp => statusGrp.Count()
                                        );
        }

        private static IEnumerable<LegalEntityVacancySummary> GetLegalEntities(IEnumerable<VacancySummaryProjection> vacancies)
        {
            return vacancies
                    .Where(v => v.LegalEntityId.HasValue && v.LegalEntityId.Value > 0)
                    .GroupBy(v => v.LegalEntityId)
                    .Select((grp) => new LegalEntityVacancySummary
                    {
                        LegalEntityId = grp.Key,
                        LegalEntityName = grp.First().LegalEntityName,
                        VacancyStatusCounts = grp
                                            .GroupBy(v => v.Status)
                                            .ToDictionary(
                                                            statusGrp => statusGrp.Key.ToString().ToLower(),
                                                            statusGrp => statusGrp.Count()
                                                        )
                    });
        }
    }
}