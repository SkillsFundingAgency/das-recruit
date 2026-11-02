using System;
using System.Linq;
using System.Threading.Tasks;
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
    public class TitleOrchestrator(
        IEmployerVacancyClient client,
        IRecruitVacancyClient vacancyClient,
        ILogger<TitleOrchestrator> logger,
        IReviewSummaryService reviewSummaryService,
        ITrainingProviderService trainingProviderService,
        IUtility utility)
        : VacancyValidatingOrchestrator<TitleEditModel>(logger)
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Title;

        public static TitleViewModel GetTitleViewModel(string employerAccountId)
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
            var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm);
            var vm = new TitleViewModel
            {
                VacancyId = vacancy.Id,
                EmployerAccountId = vrm.EmployerAccountId,
                Title = vacancy.Title,
                PageInfo = utility.GetPartOnePageInfo(vacancy),
            };
            
            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.GetValueOrDefault(),
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
                    v => vacancyClient.Validate(v, ValidationRules),
                    async v =>
                    {
                        if (m.ReferredFromSavedFavourites)
                        {
                            provider = await GetProvider(m.ReferredUkprn);
                            programme = await GetProgramme(m.ReferredProgrammeId);
                        }
                        return await client.CreateVacancyAsync(m.Title, m.EmployerAccountId, user, provider, programme?.Id);
                    });
            }

            var vacancy = await utility.GetAuthorisedVacancyForEditAsync(new VacancyRouteModel { EmployerAccountId = m.EmployerAccountId, VacancyId = m.VacancyId.Value });

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
                v => vacancyClient.Validate(v, ValidationRules),
                async v =>
                {
                    await vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                    return v.Id;
                }
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, TitleEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, TitleEditModel> { { e => e.Title, vm => vm.Title } };
            return mappings;
        }

        private async Task<TrainingProvider> GetProvider(string ukprn)
        {
            if(!long.TryParse(ukprn, out var validUkprn))
                return null;
            return await trainingProviderService.GetProviderAsync(validUkprn);
        }

        private async Task<IApprenticeshipProgramme> GetProgramme(string programmeId)
        {
            var programmesTask = await vacancyClient.GetActiveApprenticeshipProgrammesAsync(includePlaceholderProgramme: true);
            return programmesTask.SingleOrDefault(p => p.Id == programmeId);
        }
    }
}