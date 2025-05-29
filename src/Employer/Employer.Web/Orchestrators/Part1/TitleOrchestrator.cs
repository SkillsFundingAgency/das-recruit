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
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class TitleOrchestrator : VacancyValidatingOrchestrator<TitleEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Title;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly ITrainingProviderService _trainingProviderService;
        private readonly IUtility _utility;

        public TitleOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<TitleOrchestrator> logger, IReviewSummaryService reviewSummaryService, ITrainingProviderService trainingProviderService, IUtility utility) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _trainingProviderService = trainingProviderService;
            _utility = utility;
        }

        public TitleViewModel GetTitleViewModel(string employerAccountId)
        {
            var vm = new TitleViewModel
            {
                EmployerAccountId = employerAccountId,
                PageInfo = new PartOnePageInfoViewModel()
            };
            return vm;
        }

        public async Task<TitleViewModel> GetTitleViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Title_Get);
            var vm = new TitleViewModel
            {
                VacancyId = vacancy.Id,
                EmployerAccountId = vrm.EmployerAccountId,
                Title = vacancy.Title,
                PageInfo = _utility.GetPartOnePageInfo(vacancy),
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
                vm = GetTitleViewModel(m.EmployerAccountId);
            }

            vm.Title = m.Title;
            return vm;
        }

        public async Task<OrchestratorResponse<Guid>> PostTitleEditModelAsync(TitleEditModel m, VacancyUser user)
        {
            TrainingProvider provider = null;
            IApprenticeshipProgramme programme = null;
            if (!m.VacancyId.HasValue) // Create if it's a new vacancy
            {
                var newVacancy = new Vacancy
                {
                    Title = m.Title
                };
                return await ValidateAndExecute(
                    newVacancy, 
                    v => _vacancyClient.Validate(v, ValidationRules),
                    async v =>
                    {
                        if (m.ReferredFromSavedFavourites)
                        {
                            provider = await GetProvider(m.ReferredUkprn);
                            programme = await GetProgramme(m.ReferredProgrammeId);
                        }
                        return await _client.CreateVacancyAsync(m.Title, m.EmployerAccountId, user, provider, programme?.Id);
                    });
            }

            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(new VacancyRouteModel{EmployerAccountId = m.EmployerAccountId, VacancyId = m.VacancyId.Value}, RouteNames.Title_Post);

            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.Title,
                FieldIdResolver.ToFieldId(v => v.Title),
                vacancy,
                (v) =>
                {
                    return v.Title = m.Title;
                });

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

        protected override EntityToViewModelPropertyMappings<Vacancy, TitleEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, TitleEditModel>();
            mappings.Add(e => e.Title, vm => vm.Title);
            return mappings;
        }

        private async Task<TrainingProvider> GetProvider(string ukprn)
        {
            if(long.TryParse(ukprn, out long validUkprn) == false)
                return null;
            return await _trainingProviderService.GetProviderAsync(validUkprn);
        }

        public async Task<IApprenticeshipProgramme> GetProgramme(string programmeId)
        {
            var programmesTask = await _vacancyClient.GetActiveApprenticeshipProgrammesAsync();
            return programmesTask.SingleOrDefault(p => p.Id == programmeId);
        }
    }
}
