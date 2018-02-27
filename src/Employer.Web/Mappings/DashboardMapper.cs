using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using SFA.DAS.EAS.Account.Api.Types;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Employer.Web.Mappings
{
    public class DashboardMapper
    {
        public DashboardViewModel MapFromDashboard(Dashboard dashboard, AccountDetailViewModel accountDetail)
        {
            return new DashboardViewModel
            {
                EmployerName = accountDetail.DasAccountName,
                Vacancies = dashboard?.Vacancies
                                        .OrderByDescending(v => v.CreatedDate)
                                        .ToList() ?? new List<VacancySummary>()
            };
        }
    }
}
