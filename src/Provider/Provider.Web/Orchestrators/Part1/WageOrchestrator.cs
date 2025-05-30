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
    public interface IWageOrchestrator
    {
        Task<WageViewModel> GetWageViewModelAsync(VacancyRouteModel vrm);
        Task<WageViewModel> GetWageViewModelAsync(WageEditModel m);
        Task<OrchestratorResponse> PostExtraInformationEditModelAsync(WageExtraInformationViewModel m, VacancyUser user);
        Task<WageExtraInformationViewModel> GetExtraInformationViewModelAsync(VacancyRouteModel vrm);
        Task<OrchestratorResponse> PostWageEditModelAsync(WageEditModel m, VacancyUser user);
        Task<CompetitiveWageViewModel> GetCompetitiveWageViewModelAsync(VacancyRouteModel vrm);
        Task<OrchestratorResponse> PostCompetitiveWageEditModelAsync(CompetitiveWageEditModel m, VacancyUser user);
    }

    public class WageOrchestrator : VacancyValidatingOrchestrator<WageEditModel>, IWageOrchestrator
    {
        private const VacancyRuleSet CompetitiveValidationRules = VacancyRuleSet.CompetitiveWage;
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Wage;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IMinimumWageProvider _minimumWageProvider;
        private readonly IUtility _utility;

        public WageOrchestrator(IRecruitVacancyClient vacancyClient,
            ILogger<WageOrchestrator> logger, IReviewSummaryService reviewSummaryService, IMinimumWageProvider minimumWageProvider, IUtility utility)
            : base(logger)
        {
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _minimumWageProvider = minimumWageProvider;
            _utility = utility;
        }


        public async Task<OrchestratorResponse> PostExtraInformationEditModelAsync(WageExtraInformationViewModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.AddExtraInformation_Post);

            if (vacancy.Wage == null)
                vacancy.Wage = new Wage();

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.Wage.WageAdditionalInformation,
                FieldIdResolver.ToFieldId(v => v.Wage.WageAdditionalInformation),
                vacancy,
                (v) =>
                {
                    return v.Wage.WageAdditionalInformation = m.WageAdditionalInformation;
                });
            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.Wage.CompanyBenefitsInformation,
                FieldIdResolver.ToFieldId(v => v.Wage.CompanyBenefitsInformation),
                vacancy,
                (v) =>
                {
                    return v.Wage.CompanyBenefitsInformation = m.CompanyBenefitsInformation;
                });
            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        public async Task<WageExtraInformationViewModel> GetExtraInformationViewModelAsync(VacancyRouteModel vrm)
        {
            var vm = await GetWageViewModelAsync(vrm);

            var wageExtraInformationViewModel = new WageExtraInformationViewModel
            {
                VacancyId = vm.VacancyId,
                PageInfo = vm.PageInfo,
                WageType = vm.WageType,
                WageAdditionalInformation = vm.WageAdditionalInformation,
                Ukprn = vm.Ukprn,
                CompanyBenefitsInformation = vm.CompanyBenefitsInformation,
                Title = vm.Title,
                Review = vm.Review,
                IsTaskListCompleted = vm.IsTaskListCompleted
            };

            return wageExtraInformationViewModel;
        }

        public async Task<CompetitiveWageViewModel> GetCompetitiveWageViewModelAsync(VacancyRouteModel vrm)
        {
            var vm = await GetWageViewModelAsync(vrm);

            var competitiveWageViewModel = new CompetitiveWageViewModel
            {
                VacancyId = vm.VacancyId,
                WageType = vm.WageType,
                FixedWageYearlyAmount = vm.FixedWageYearlyAmount,
                WageAdditionalInformation = vm.WageAdditionalInformation,
                MinimumWageStartFrom = vm.MinimumWageStartFrom,
                NationalMinimumWageLowerBoundHourly = vm.NationalMinimumWageLowerBoundHourly,
                NationalMinimumWageUpperBoundHourly = vm.NationalMinimumWageUpperBoundHourly,
                NationalMinimumWageYearly = vm.NationalMinimumWageYearly,
                ApprenticeshipMinimumWageHourly = vm.ApprenticeshipMinimumWageHourly,
                ApprenticeshipMinimumWageYearly = vm.ApprenticeshipMinimumWageYearly,
                WeeklyHours = vm.WeeklyHours,
                PageInfo = vm.PageInfo,
                Review = vm.Review,
                Ukprn = vrm.Ukprn,
                IsSalaryAboveNationalMinimumWage = (vm.WageType == WageType.CompetitiveSalary) ? true : null,
                Title = vm.Title,
            };

            return competitiveWageViewModel;
        }

        public async Task<OrchestratorResponse> PostCompetitiveWageEditModelAsync(CompetitiveWageEditModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.SetCompetitivePayRate_Post);

            if (vacancy.Wage == null)
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
                v => _vacancyClient.Validate(v, CompetitiveValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        public async Task<WageViewModel> GetWageViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Wage_Get);

            var wagePeriod = _minimumWageProvider.GetWagePeriod(vacancy.StartDate.Value);

            var vm = new WageViewModel
            {
                Title = vacancy.Title,
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
                PageInfo = _utility.GetPartOnePageInfo(vacancy),
                Ukprn = vrm.Ukprn,
                VacancyId = vrm.VacancyId,
                CompanyBenefitsInformation = vacancy.Wage?.CompanyBenefitsInformation,
                IsTaskListCompleted = _utility.IsTaskListCompleted(vacancy)
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetAdditionalWageInformationFieldIndicators());
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
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.Wage_Post);

            if (vacancy.Wage == null)
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
            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.Wage.CompanyBenefitsInformation,
                FieldIdResolver.ToFieldId(v => v.Wage.CompanyBenefitsInformation),
                vacancy,
                (v) =>
                {
                    return v.Wage.CompanyBenefitsInformation = m.CompanyBenefitsInformation;
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
            mappings.Add(e => e.Wage.CompanyBenefitsInformation, vm => vm.CompanyBenefitsInformation);

            return mappings;
        }
    }
}