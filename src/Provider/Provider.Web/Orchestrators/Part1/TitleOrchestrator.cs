using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Title;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Provider.Web.ViewModels.Part1;
using Esfa.Recruit.Shared.Web.Orchestrators;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class TitleOrchestrator : EntityValidatingOrchestrator<Vacancy, TitleEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Title | VacancyRuleSet.NumberOfPositions;
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public TitleOrchestrator(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<TitleOrchestrator> logger, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
        }

        public TitleViewModel GetTitleViewModel()
        {
            var vm = new TitleViewModel
            {
                PageInfo = new PartOnePageInfoViewModel()
            };
            return vm;
        }

        public async Task<TitleViewModel> GetTitleViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Title_Get);

            var vm = new TitleViewModel
            {
                VacancyId = vacancy.Id,
                Title = vacancy.Title,
                NumberOfPositions = vacancy.NumberOfPositions?.ToString(),
                PageInfo = Utility.GetPartOnePageInfo(vacancy)
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                // vm.Review = await _reviewSummaryService.GetReviewSummaryViewModel(vacancy.VacancyReference.Value,
                //     ReviewFieldMappingLookups.GetTitleFieldIndicators());
            }

            return vm;
        }

        public async Task<OrchestratorResponse<Guid>> PostTitleEditModelAsync(TitleEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, 
                new VacancyRouteModel{Ukprn = m.Ukprn, VacancyId = m.VacancyId.Value}, RouteNames.Title_Post);

            vacancy.Title = m.Title;

            var numberOfPositions = int.TryParse(m.NumberOfPositions, out var n)? n : default(int?);            
            vacancy.NumberOfPositions = numberOfPositions;

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
            mappings.Add(e => e.NumberOfPositions, vm => vm.NumberOfPositions);

            return mappings;
        }
    }
}
