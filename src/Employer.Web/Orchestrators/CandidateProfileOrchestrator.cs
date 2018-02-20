using Esfa.Recruit.Employer.Web.ViewModels.CandidateProfile;
using Esfa.Recruit.Storage.Client.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class CandidateProfileOrchestrator
    {
        private readonly IQueryVacancyRepository _queryRepository;

        public CandidateProfileOrchestrator(IQueryVacancyRepository queryRepository)
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
