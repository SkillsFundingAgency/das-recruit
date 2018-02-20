using Esfa.Recruit.Employer.Web.ViewModels.Submitted;
using Esfa.Recruit.Storage.Client.Domain.QueryStore;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class SubmittedOrchestrator
    {
        private readonly IQueryStoreReader _queryRepository;

        public SubmittedOrchestrator(IQueryStoreReader queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<IndexViewModel> GetIndexViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _queryRepository.GetVacancyForEditAsync(vacancyId);

            var vm = new IndexViewModel
            {
                Title = vacancy.Title,
                VacancyReference = "12345678"
            };

            return vm;
        }
    }
}
