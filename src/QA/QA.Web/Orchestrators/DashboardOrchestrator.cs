using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
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
                    Status = CalculateStatus(review)
                });
            }

            return vm;
        }

        private static string CalculateStatus(VacancyReview review)
        {
            return review.Status == ReviewStatus.UnderReview ? review.Status.GetDisplayName() : "Submitted";
        }
    }
}