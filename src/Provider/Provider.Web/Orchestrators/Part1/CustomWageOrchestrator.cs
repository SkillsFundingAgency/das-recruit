using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.CustomWage;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.VacancyServices.Wage;
using WageType = Esfa.Recruit.Vacancies.Client.Domain.Entities.WageType;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public interface ICustomWageOrchestrator
    {
        Task<CustomWageViewModel> GetCustomWageViewModelAsync(VacancyRouteModel vrm);
        Task<CustomWageViewModel> GetCustomWageViewModelAsync(CustomWageEditModel m);
        Task<OrchestratorResponse> PostCustomWageEditModelAsync(CustomWageEditModel m, VacancyUser user);
    }

    public class CustomWageOrchestrator : VacancyValidatingOrchestrator<CustomWageEditModel>, ICustomWageOrchestrator
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Wage | VacancyRuleSet.MinimumWage;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IMinimumWageProvider _minimumWageProvider;
        private readonly IUtility _utility;

        public CustomWageOrchestrator(IRecruitVacancyClient vacancyClient, ILogger<WageOrchestrator> logger, IReviewSummaryService reviewSummaryService, IMinimumWageProvider minimumWageProvider, IUtility utility) : base(logger)
        {
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _minimumWageProvider = minimumWageProvider;
            _utility = utility;
        }

        public async Task<CustomWageViewModel> GetCustomWageViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Wage_Get);
            var wagePeriod = _minimumWageProvider.GetWagePeriod(vacancy.StartDate.Value);

            var vm = new CustomWageViewModel
            {
                VacancyId = vacancy.Id,
                FixedWageYearlyAmount = vacancy.Wage?.FixedWageYearlyAmount?.AsMoney(),
                WageAdditionalInformation = vacancy.Wage?.WageAdditionalInformation,
                MinimumWageStartFrom = wagePeriod.ValidFrom.ToDayMonthYearString(),
                NationalMinimumWageLowerBoundHourly = wagePeriod.NationalMinimumWageLowerBound.ToString("C"),
                NationalMinimumWageUpperBoundHourly = wagePeriod.NationalMinimumWageUpperBound.ToString("C"),
                NationalMinimumWageYearly = GetMinimumWageYearlyText(SFA.DAS.VacancyServices.Wage.WageType.NationalMinimum, vacancy.Wage?.WeeklyHours, vacancy.StartDate.Value),
                ApprenticeshipMinimumWageHourly = wagePeriod.ApprenticeshipMinimumWage.ToString("C"),
                ApprenticeshipMinimumWageYearly = GetMinimumWageYearlyText(SFA.DAS.VacancyServices.Wage.WageType.ApprenticeshipMinimum, vacancy.Wage?.WeeklyHours, vacancy.StartDate.Value),
                WeeklyHours = vacancy.Wage.WeeklyHours.Value,
                Ukprn = vrm.Ukprn,
                PageInfo = _utility.GetPartOnePageInfo(vacancy),
                VacancyTitle = vacancy.Title,
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetWageReviewFieldIndicators());
            }

            return vm;
        }

        public async Task<CustomWageViewModel> GetCustomWageViewModelAsync(CustomWageEditModel m)
        {
            var vm = await GetCustomWageViewModelAsync((VacancyRouteModel)m);
            vm.FixedWageYearlyAmount = m.FixedWageYearlyAmount;
            return vm;
        }

        public async Task<OrchestratorResponse> PostCustomWageEditModelAsync(CustomWageEditModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.CustomWage_Post);

            if (vacancy.Wage == null)
                vacancy.Wage = new Wage();

            if (vacancy.Wage.WageType != WageType.FixedWage)
                SetVacancyWithProviderReviewFieldIndicators(
                    vacancy.Wage.WageAdditionalInformation,
                    FieldIdResolver.ToFieldId(v => v.Wage.WageAdditionalInformation),
                    vacancy,
                    (v) =>
                    {
                        return v.Wage.WageAdditionalInformation = null;
                    });

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.Wage.WageType,
                FieldIdResolver.ToFieldId(v => v.Wage.WageType),
                vacancy,
                (v) =>
                {
                    return v.Wage.WageType = WageType.FixedWage;
                });

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.Wage.FixedWageYearlyAmount,
                FieldIdResolver.ToFieldId(v => v.Wage.FixedWageYearlyAmount),
                vacancy,
                (v) =>
                {
                    return v.Wage.FixedWageYearlyAmount = m.FixedWageYearlyAmount?.AsMoney();
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

        protected override EntityToViewModelPropertyMappings<Vacancy, CustomWageEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, CustomWageEditModel>();
            mappings.Add(e => e.Wage.FixedWageYearlyAmount, vm => vm.FixedWageYearlyAmount);
            return mappings;
        }
    }
}
