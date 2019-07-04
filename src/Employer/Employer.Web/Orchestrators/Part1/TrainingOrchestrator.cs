using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class TrainingOrchestrator : EntityValidatingOrchestrator<Vacancy, TrainingEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingProgramme;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public TrainingOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<TrainingOrchestrator> logger, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
        }
        
        public async Task<TrainingViewModel> GetTrainingViewModelAsync(VacancyRouteModel vrm, VacancyUser user)
        {
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Training_Get);
            var programmesTask = _vacancyClient.GetActiveApprenticeshipProgrammesAsync();
            var isUsersFirstVacancyTask = IsUsersFirstVacancy(user.UserId);

            await Task.WhenAll(vacancyTask, programmesTask, isUsersFirstVacancyTask);

            var vacancy = vacancyTask.Result;
            var programmes = programmesTask.Result;
            
            var vm = new TrainingViewModel
            {
                VacancyId = vacancy.Id,
                SelectedProgrammeId = vacancy.ProgrammeId,
                Programmes = programmes.ToViewModel(),
                IsUsersFirstVacancy = isUsersFirstVacancyTask.Result && vacancy.TrainingProvider == null,
                PageInfo = Utility.GetPartOnePageInfo(vacancy)
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetTrainingReviewFieldIndicators());
            }

            return vm;
        }

        public async Task<TrainingViewModel> GetTrainingViewModelAsync(TrainingEditModel m, VacancyUser user)
        {
            var vm = await GetTrainingViewModelAsync((VacancyRouteModel)m, user);

            vm.SelectedProgrammeId = m.SelectedProgrammeId;

            return vm;
        }

        public async Task<OrchestratorResponse> PostTrainingEditModelAsync(TrainingEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.Training_Post);

            vacancy.ProgrammeId = m.SelectedProgrammeId;
            
            return await ValidateAndExecute(
                vacancy, 
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        public async Task<TrainingFirstVacancyViewModel> GetTrainingFirstVacancyViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Training_First_Time_Get);

            return new TrainingFirstVacancyViewModel();
        }

        private async Task<bool> IsUsersFirstVacancy(string userId)
        {
            var userVacancies = await _client.GetVacanciesForUserAsync(userId);

            return userVacancies.Count() == 1;
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, TrainingEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, TrainingEditModel>();

            mappings.Add(e => e.ProgrammeId, vm => vm.SelectedProgrammeId);

            return mappings;
        }
    }
}
