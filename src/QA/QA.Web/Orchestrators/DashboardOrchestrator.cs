using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Extensions;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Qa.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private readonly IQaVacancyClient _vacancyClient;

        public DashboardOrchestrator(IQaVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync()
        {            
            var reviews = await _vacancyClient.GetDashboardAsync();
            var vm = MapToViewModel(reviews);
            return vm;
        }

        private static DashboardViewModel MapToViewModel(IEnumerable<VacancyReview> reviews)
        {
            var vm = new DashboardViewModel();

            foreach(var review in reviews)
            {
                vm.Add(new ReviewDashboardItem 
                { 
                    ReviewId = review.Id,
                    VacancyReference = review.VacancyReference,
                    Title = review.Title,
                    Status = review.ManualOutcome?.GetDisplayName() ?? "Submitted"
                });
            }

            return vm;
        }
    }
}