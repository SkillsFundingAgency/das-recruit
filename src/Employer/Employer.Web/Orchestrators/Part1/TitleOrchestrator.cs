using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class TitleOrchestrator : EntityValidatingOrchestrator<Vacancy, TitleEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Title | VacancyRuleSet.NumberOfPositions;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public TitleOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<TitleOrchestrator> logger, IReviewSummaryService reviewSummaryService) : base(logger)
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
                vm = GetTitleViewModel();
            }

            vm.Title = m.Title;
            vm.NumberOfPositions = m.NumberOfPositions;

            return vm;
        }


        public async Task<OrchestratorResponse<Guid>> PostTitleEditModelAsync(TitleEditModel m, VacancyUser user)
        {
            var numberOfPositions = int.TryParse(m.NumberOfPositions, out var n)? n : default(int?);

            if (!m.VacancyId.HasValue) // Create if it's a new vacancy
            {
                var newVacancy = new Vacancy
                {
                    Title = m.Title,
                    NumberOfPositions = numberOfPositions
                };

                return await ValidateAndExecute(
                    newVacancy, 
                    v => _vacancyClient.Validate(v, ValidationRules),
                    async v => await _client.CreateVacancyAsync(m.Title, numberOfPositions.Value, m.EmployerAccountId, user));
            }

            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, 
                new VacancyRouteModel{EmployerAccountId = m.EmployerAccountId, VacancyId = m.VacancyId.Value}, RouteNames.Title_Post);

            vacancy.Title = m.Title;
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
