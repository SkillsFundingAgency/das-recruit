using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class TitleOrchestrator : EntityValidatingOrchestrator<Vacancy, TitleEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Title;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IEmployerVacancyClient _employerVacancyClient;

        public TitleOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<TitleOrchestrator> logger, IReviewSummaryService reviewSummaryService, IEmployerVacancyClient employerVacancyClient) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _employerVacancyClient = employerVacancyClient;
        }

        public async Task<TitleViewModel> GetTitleViewModel(string employerAccountId)
        {
            var dashboard = await _employerVacancyClient.GetDashboardAsync(employerAccountId);
            var vm = new TitleViewModel
            {
                PageInfo = new PartOnePageInfoViewModel(),
                ShowReturnToMALink = dashboard == null || !dashboard.CloneableVacancies.Any()
            };
            return vm;
        }

        public async Task<TitleViewModel> GetTitleViewModelAsync(VacancyRouteModel vrm)
        {
            var dashboard = await _employerVacancyClient.GetDashboardAsync(vrm.EmployerAccountId);
            
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Title_Get);
            var vm = new TitleViewModel
            {
                VacancyId = vacancy.Id,
                Title = vacancy.Title,
                PageInfo = Utility.GetPartOnePageInfo(vacancy),
                HasCloneableVacancies = dashboard.CloneableVacancies.Any()
            };
            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetTitleFieldIndicators());
            }

            return vm;
        }

        public async Task<TitleViewModel> GetTitleViewModelAsync(TitleEditModel m)
        {
            TitleViewModel vm;

            if (m.VacancyId.HasValue)
            {
                var vrm = new VacancyRouteModel { EmployerAccountId = m.EmployerAccountId, VacancyId = m.VacancyId.Value };
                vm = await GetTitleViewModelAsync(vrm);
            }
            else
            {
                vm = await GetTitleViewModel(m.EmployerAccountId);
            }

            vm.Title = m.Title;
            return vm;
        }


        public async Task<OrchestratorResponse<Guid>> PostTitleEditModelAsync(TitleEditModel m, VacancyUser user)
        {
            TrainingProvider provider = null;
            IApprenticeshipProgramme programme = null;
            if(IsReferredFromMaHomeFavourites(m))
            {
                provider = await GetProvider(m.ReferredFromMAHome_UKPRN);
                programme = await GetProgramme(m.ReferredFromMAHome_ProgrammeId);
            }
            if (!m.VacancyId.HasValue) // Create if it's a new vacancy
            {
                var newVacancy = new Vacancy
                {
                    Title = m.Title
                };

                return await ValidateAndExecute(
                    newVacancy, 
                    v => _vacancyClient.Validate(v, ValidationRules),
                    async v => await _client.CreateVacancyAsync(m.Title, m.EmployerAccountId, user, provider, programme?.Id));
            }

            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, 
                new VacancyRouteModel{EmployerAccountId = m.EmployerAccountId, VacancyId = m.VacancyId.Value}, RouteNames.Title_Post);

            vacancy.Title = m.Title;

            return await ValidateAndExecute(
                vacancy, 
                v => _vacancyClient.Validate(v, ValidationRules),
                async v =>
                {
                    await _vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                    return v.Id;
                }
            );
        }

        private bool IsReferredFromMaHomeFavourites(TitleEditModel m)
        {
            return m.ReferredFromMAHome && (!string.IsNullOrWhiteSpace(m.ReferredFromMAHome_ProgrammeId) || !string.IsNullOrWhiteSpace(m.ReferredFromMAHome_UKPRN));
        }

        private async Task<TrainingProvider> GetProvider(string ukprn)
        {
            long.TryParse(ukprn, out long validUkprn);
            if(validUkprn != 0)
                return await _client.GetTrainingProviderAsync(validUkprn);
            return null;
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, TitleEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, TitleEditModel>();
            mappings.Add(e => e.Title, vm => vm.Title);
            return mappings;
        }

        private async Task<IApprenticeshipProgramme> GetProgramme(string programmeId)
        {
            var programmesTask = await _vacancyClient.GetActiveApprenticeshipProgrammesAsync();
            return programmesTask.SingleOrDefault(p => p.Id == programmeId);
        }
    }
}
