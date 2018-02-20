using Esfa.Recruit.Employer.Web.ViewModels.TrainingProvider;
using Esfa.Recruit.Storage.Client.Domain.QueryStore;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class TrainingProviderOrchestrator
    {
        private readonly IQueryStoreReader _queryRepository;

        public TrainingProviderOrchestrator(IQueryStoreReader queryRepository)
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

        public async Task<ConfirmViewModel> GetConfirmViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _queryRepository.GetVacancyForEditAsync(vacancyId);

            var vm = new ConfirmViewModel
            {
                Title = vacancy.Title
            };

            return vm;
        }
    }
}
