using Esfa.Recruit.Employer.Web.ViewModels.ApplicationProcess;
using Esfa.Recruit.Storage.Client.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class ApplicationProcessOrchestrator
    {
        private readonly IQueryVacancyRepository _queryRepository;

        public ApplicationProcessOrchestrator(IQueryVacancyRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<IndexViewModel> GetIndexViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _queryRepository.GetVacancyAsync(vacancyId);

            var vm = new IndexViewModel
            {
                Title = vacancy.Title
            };

            return vm;
        }
    }
}
