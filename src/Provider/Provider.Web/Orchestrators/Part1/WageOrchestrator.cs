using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using SFA.DAS.VacancyServices.Wage;
using WageType = Esfa.Recruit.Vacancies.Client.Domain.Entities.WageType;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class WageOrchestrator : VacancyValidatingOrchestrator<WageEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Wage | VacancyRuleSet.MinimumWage;
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IMinimumWageProvider _minimumWageProvider;

        public WageOrchestrator(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient, 
            ILogger<WageOrchestrator> logger, IReviewSummaryService reviewSummaryService, IMinimumWageProvider minimumWageProvider) 
            : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _minimumWageProvider = minimumWageProvider;
        }

        public async Task<WageViewModel> GetWageViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _client, _vacancyClient, vrm, RouteNames.Wage_Get);

            var wagePeriod = _minimumWageProvider.GetWagePeriod(vacancy.StartDate.Value);

            var vm = new WageViewModel
            {
                WageType = vacancy.Wage?.WageType,
                FixedWageYearlyAmount = vacancy.Wage?.FixedWageYearlyAmount?.AsMoney(),
                WageAdditionalInformation = vacancy.Wage?.WageAdditionalInformation,
                MinimumWageStartFrom = wagePeriod.ValidFrom.ToDayMonthYearString(),
                NationalMinimumWageLowerBoundHourly = wagePeriod.NationalMinimumWageLowerBound.ToString("C"),
                NationalMinimumWageUpperBoundHourly = wagePeriod.NationalMinimumWageUpperBound.ToString("C"),
                NationalMinimumWageYearly = GetMinimumWageYearlyText(SFA.DAS.VacancyServices.Wage.WageType.NationalMinimum, vacancy.Wage?.WeeklyHours, vacancy.StartDate.Value),
                ApprenticeshipMinimumWageHourly = wagePeriod.ApprenticeshipMinimumWage.ToString("C"),
                ApprenticeshipMinimumWageYearly = GetMinimumWageYearlyText(SFA.DAS.VacancyServices.Wage.WageType.ApprenticeshipMinimum, vacancy.Wage?.WeeklyHours, vacancy.StartDate.Value),
                WeeklyHours = vacancy.Wage.WeeklyHours.Value,
                PageInfo = Utility.GetPartOnePageInfo(vacancy)
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetWageReviewFieldIndicators());
            }

            return vm;
        }

        public async Task<WageViewModel> GetWageViewModelAsync(WageEditModel m)
        {
            var vm = await GetWageViewModelAsync((VacancyRouteModel)m);

            vm.WageType = m.WageType;
            vm.FixedWageYearlyAmount = m.FixedWageYearlyAmount;
            vm.WageAdditionalInformation = m.WageAdditionalInformation;
            
            return vm;
        }

        public async Task<OrchestratorResponse> PostWageEditModelAsync(WageEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.Wage_Post);

            if(vacancy.Wage == null)
                vacancy.Wage = new Wage();

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.Wage.WageType,
                FieldIdResolver.ToFieldId(v => v.Wage.WageType),
                vacancy,
                (v) =>
                {
                    return v.Wage.WageType = m.WageType;
                });

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.Wage.FixedWageYearlyAmount,
                FieldIdResolver.ToFieldId(v => v.Wage.FixedWageYearlyAmount),
                vacancy,
                (v) =>
                {
                    return v.Wage.FixedWageYearlyAmount = (m.WageType == WageType.FixedWage) ? m.FixedWageYearlyAmount?.AsMoney() : null;
                });

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.Wage.WageAdditionalInformation,
                FieldIdResolver.ToFieldId(v => v.Wage.WageAdditionalInformation),
                vacancy,
                (v) =>
                {
                    return v.Wage.WageAdditionalInformation = m.WageAdditionalInformation;
                });

            return await ValidateAndExecute(
                vacancy, 
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        private string GetMinimumWageYearlyText(SFA.DAS.VacancyServices.Wage.WageType wageType, decimal? weeklyHours, DateTime startDate)
        {
            return WagePresenter.GetDisplayText(wageType, WageUnit.Annually, new WageDetails
            {
                HoursPerWeek = weeklyHours,
                StartDate = startDate
            }).AsWholeMoneyAmount();
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, WageEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, WageEditModel>();

            mappings.Add(e => e.Wage.WageType, vm => vm.WageType);
            mappings.Add(e => e.Wage.FixedWageYearlyAmount, vm => vm.FixedWageYearlyAmount);
            mappings.Add(e => e.Wage.WageAdditionalInformation, vm => vm.WageAdditionalInformation);

            return mappings;
        }
    }
}