using Esfa.Recruit.Employer.Web.ViewModels.WageAndHours;
using Esfa.Recruit.Storage.Client.Domain.QueryStore;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class WageAndHoursOrchestrator
    {
        private readonly IQueryStoreReader _queryRepository;

        public WageAndHoursOrchestrator(IQueryStoreReader queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<IndexViewModel> GetIndexViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _queryRepository.GetVacancyForEditAsync(vacancyId);

            var vm = new IndexViewModel
            {
                Title = vacancy.Title
            };

            return vm;
        }
    }
}
