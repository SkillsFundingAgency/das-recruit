using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Title;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class TitleOrchestrator : VacancyValidatingOrchestrator<TitleEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Title;
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public TitleOrchestrator(IProviderVacancyClient providerVacancyClient, IRecruitVacancyClient recruitVacancyClient, 
            ILogger<TitleOrchestrator> logger, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _providerVacancyClient = providerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<TitleViewModel> GetTitleViewModelForNewVacancyAsync(string employerAccountId, long ukprn)
        {
            await ValidateEmployerAccountIdAsync(ukprn, employerAccountId);
            var dashboard = await _providerVacancyClient.GetDashboardAsync(ukprn);
            var vm = new TitleViewModel
            {
                EmployerAccountId = employerAccountId,
                Ukprn = ukprn,
                PageInfo = new PartOnePageInfoViewModel(),
                HasAnyVacancies = dashboard.Vacancies.Any()
            };
            return vm;
        }

        public async Task<TitleViewModel> GetTitleViewModelForExistingVacancyAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_providerVacancyClient, _recruitVacancyClient, vrm, RouteNames.Title_Get);
            var ukprn = vacancy.TrainingProvider.Ukprn.GetValueOrDefault();
            var dashboard = await _providerVacancyClient.GetDashboardAsync(ukprn);
            var vm = new TitleViewModel
            {
                    VacancyId = vacancy.Id,
                    Title = vacancy.Title,
                    PageInfo = Utility.GetPartOnePageInfo(vacancy),
                    Ukprn = ukprn,
                    EmployerAccountId = vacancy.EmployerAccountId,
                    HasAnyVacancies = dashboard.Vacancies.Any()
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetTitleFieldIndicators());
            }

            return vm;
        }

        public async Task<TitleViewModel> GetTitleViewModelFromEditModelAsync(VacancyRouteModel vrm, TitleEditModel model, long ukprn)
        {
            TitleViewModel vm;

            if(model.VacancyId.HasValue)
            {
                vm = await GetTitleViewModelForExistingVacancyAsync(vrm);
            }
            else
            {
                vm = await GetTitleViewModelForNewVacancyAsync(model.EmployerAccountId, ukprn);                
            }

            vm.Title = model.Title;

            return vm;
        }

        public async Task<OrchestratorResponse<Guid>> PostTitleEditModelAsync(VacancyRouteModel vrm, TitleEditModel model, VacancyUser user, long ukprn)
        {
            if (model.VacancyId.HasValue) 
            {
                var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_providerVacancyClient, _recruitVacancyClient, vrm, RouteNames.Title_Post);

                SetVacancyWithProviderReviewFieldIndicators(
                vacancy.Title,
                FieldIdResolver.ToFieldId(v => v.Title),
                vacancy,
                (v) =>
                {
                    return v.Title = model.Title;
                });

                return await ValidateAndExecute(
                    vacancy, 
                    v => _recruitVacancyClient.Validate(v, ValidationRules),
                    async v =>
                    {
                        await _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                        return v.Id;
                    }
                );
            }

            await ValidateEmployerAccountIdAsync(ukprn, model.EmployerAccountId);

            var newVacancy = new Vacancy
            {
                TrainingProvider = new TrainingProvider { Ukprn = ukprn },
                Title = model.Title
            };

            return await ValidateAndExecute(
                newVacancy,
                v => _recruitVacancyClient.Validate(v, ValidationRules),
                async v => await _providerVacancyClient.CreateVacancyAsync(
                        model.EmployerAccountId, ukprn, model.Title, user));
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, TitleEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, TitleEditModel>();

            mappings.Add(e => e.Title, vm => vm.Title);
            mappings.Add(e => e.EmployerName, vm => vm.EmployerAccountId);

            return mappings;
        }

        private async Task ValidateEmployerAccountIdAsync(long ukprn, string employerAccountId)
        {
            var providerInfo = await _providerVacancyClient.GetProviderEditVacancyInfoAsync(ukprn);

            if (providerInfo.Employers.Any(e => e.EmployerAccountId == employerAccountId) == false)
                throw new AuthorisationException(string.Format(ExceptionMessages.ProviderEmployerAccountIdNotFound, ukprn, employerAccountId));
        }
    }
}
