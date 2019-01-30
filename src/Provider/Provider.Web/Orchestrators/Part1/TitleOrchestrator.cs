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
using Esfa.Recruit.Provider.Web.Extensions;
using System.Linq;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class TitleOrchestrator : EntityValidatingOrchestrator<Vacancy, TitleEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Title | VacancyRuleSet.NumberOfPositions;
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

        public TitleViewModel GetTitleViewModelForNewVacancy(string employerAccountId, long ukprn)
        {
            var vm = new TitleViewModel
            {
                EmployerAccountId = employerAccountId,
                Ukprn = ukprn,
                PageInfo = new PartOnePageInfoViewModel()
            };
            return vm;
        }

        public async Task<TitleViewModel> GetTitleViewModelAsync(TitleEditModel model, long ukprn)
        {
            if(!model.VacancyId.HasValue)
            {
                return new TitleViewModel
                {
                    VacancyId = model.VacancyId,
                    Title = model.Title,
                    NumberOfPositions = model.NumberOfPositions?.ToString(),
                    PageInfo = new PartOnePageInfoViewModel(),
                    Ukprn = ukprn,
                    EmployerAccountId = model.EmployerAccountId
                };                
            }

            var vrm = new VacancyRouteModel { VacancyId = model.VacancyId, Ukprn = ukprn };
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_providerVacancyClient, _recruitVacancyClient, vrm, RouteNames.Title_Get);

            return new TitleViewModel
            {
                    VacancyId = vacancy.Id,
                    Title = model.Title,
                    NumberOfPositions = model.NumberOfPositions?.ToString(),
                    PageInfo = Utility.GetPartOnePageInfo(vacancy),
                    Ukprn = ukprn,
                    EmployerAccountId = model.EmployerAccountId
            };
            // if (vacancy.Status == VacancyStatus.Referred)
            // {
            //     vm.Review = await _reviewSummaryService.GetReviewSummaryViewModel(vacancy.VacancyReference.Value,
            //         ReviewFieldMappingLookups.GetTitleFieldIndicators());
            // }            
        }

        public async Task<OrchestratorResponse<Guid>> PostTitleEditModelAsync(TitleEditModel model, VacancyUser user, long ukprn)
        {
            if (!model.VacancyId.HasValue) // Create if it's a new vacancy
            {
                return await ValidateAndCreateVacancyAsync(model, user, ukprn);
            }

            //vacancy.EmployerName = await GetEmployerNameAsync(viewModel.Ukprn, viewModel.SelectedEmployerId);

            // var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_recruitVacancyClient, _providerVacancyClient, 
            //     new VacancyRouteModel{Ukprn = m.Ukprn, VacancyId = m.VacancyId.Value}, RouteNames.Title_Post);

            // vacancy.Title = m.Title;


            // vacancy.NumberOfPositions = numberOfPositions;

            // return await ValidateAndExecute(
            //     vacancy, 
            //     v => _vacancyClient.Validate(v, ValidationRules),
            //     async v =>
            //     {
            //         await _vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
            //         return v.Id;
            //     }
            // );
            throw new NotImplementedException();
        }

        private async Task<OrchestratorResponse<Guid>> ValidateAndCreateVacancyAsync(TitleEditModel model, VacancyUser user, long ukprn)
        {
            int.TryParse(model.NumberOfPositions, out var numberOfPositions);

            var employerName = await GetEmployerNameAsync(ukprn, model.EmployerAccountId);

            var newVacancy = new Vacancy
            {
                TrainingProvider = new TrainingProvider { Ukprn = ukprn },
                Title = model.Title,
                NumberOfPositions = numberOfPositions
            };

            return await ValidateAndExecute(
                newVacancy,
                v => _recruitVacancyClient.Validate(v, ValidationRules),
                async v => await _providerVacancyClient.CreateVacancyAsync(
                    model.EmployerAccountId, employerName, ukprn, model.Title, numberOfPositions, user));
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, TitleEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, TitleEditModel>();

            mappings.Add(e => e.Title, vm => vm.Title);
            mappings.Add(e => e.NumberOfPositions, vm => vm.NumberOfPositions);
            mappings.Add(e => e.EmployerName, vm => vm.EmployerAccountId);

            return mappings;
        }

        private async Task<string> GetEmployerNameAsync(long ukprn, string employerId)
        {
            var providerInfo = await _providerVacancyClient.GetProviderEditVacancyInfoAsync(ukprn);

            return providerInfo.Employers.FirstOrDefault(e => e.Id == employerId)?.Name;
        }
    }
}
