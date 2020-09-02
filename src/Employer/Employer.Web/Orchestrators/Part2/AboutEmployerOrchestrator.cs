using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.AboutEmployer;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class AboutEmployerOrchestrator : EntityValidatingOrchestrator<Vacancy, AboutEmployerEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerDescription | VacancyRuleSet.EmployerWebsiteUrl;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public AboutEmployerOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<AboutEmployerOrchestrator> logger, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<AboutEmployerViewModel> GetAboutEmployerViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.AboutEmployer_Get);

            var vm = new AboutEmployerViewModel
            {
                Title = vacancy.Title,
                EmployerDescription =  await _vacancyClient.GetEmployerDescriptionAsync(vacancy),
                EmployerTitle = await GetEmployerTitleAsync(vacancy),
                EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl,
                IsAnonymous = vacancy.IsAnonymous
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetAboutEmployerFieldIndicators());
            }

            return vm;
        }

        public async Task<AboutEmployerViewModel> GetAboutEmployerViewModelAsync(AboutEmployerEditModel m)
        {
            var vm = await GetAboutEmployerViewModelAsync((VacancyRouteModel)m);

            vm.EmployerDescription = m.EmployerDescription;
            vm.EmployerWebsiteUrl = m.EmployerWebsiteUrl;

            return vm;
        }

        public async Task<OrchestratorResponse> PostAboutEmployerEditModelAsync(AboutEmployerEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.AboutEmployer_Post);

            vacancy.EmployerDescription = m.EmployerDescription;
            vacancy.EmployerWebsiteUrl = m.EmployerWebsiteUrl;

            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                async v =>    
                {
                    vacancy.EmployerDescription = null; // We don't want to save the description until submission.
                    await _vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                    await UpdateEmployerProfileAsync(vacancy, m.EmployerDescription, user);
                }
            );
        }

        private async Task UpdateEmployerProfileAsync(Vacancy vacancy, string employerDescription, VacancyUser user)
        {
            var employerProfile =
                await _vacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);

            if (employerProfile == null)
            {
                throw new NullReferenceException($"No Employer Profile was found for employerAccount: {vacancy.EmployerAccountId}, " +
                                                 $"accountLegalEntityPublicHashedId : {vacancy.AccountLegalEntityPublicHashedId}");
            }

            if (employerProfile.AboutOrganisation != employerDescription)
            {
                employerProfile.AboutOrganisation = employerDescription;
                await _vacancyClient.UpdateEmployerProfileAsync(employerProfile, user);
            }
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, AboutEmployerEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, AboutEmployerEditModel>();

            mappings.Add(e => e.EmployerDescription, vm => vm.EmployerDescription);
            mappings.Add(e => e.EmployerWebsiteUrl, vm => vm.EmployerWebsiteUrl);

            return mappings;
        }

        private async Task<string> GetEmployerTitleAsync(Vacancy vacancy)
        {
            if (vacancy.IsAnonymous)
                return vacancy.LegalEntityName;

            return await _vacancyClient.GetEmployerNameAsync(vacancy);
        }
    }
}
