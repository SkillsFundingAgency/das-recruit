using Esfa.Recruit.Employer.Web.ViewModels.ApprenticeshipDetails;
using Esfa.Recruit.Storage.Client.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class ApprenticeshipDetailsOrchestrator
    {
        private readonly IQueryVacancyRepository _queryRepository;

        public ApprenticeshipDetailsOrchestrator(IQueryVacancyRepository queryRepository)
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
