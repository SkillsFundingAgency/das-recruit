using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels.Dashboard;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class AlertViewModelService
    {
        public BlockedProviderAlertViewModel GetBlockedProviderVacanciesAlert(IEnumerable<VacancySummary> vacancies, DateTime? userLastDismissedDate)
        {
            if (userLastDismissedDate.HasValue == false)
                userLastDismissedDate = DateTime.MinValue;

            var blockedProviderVacancies = GetClosedVacancies(vacancies, userLastDismissedDate, ClosureReason.BlockedByQa);

            if (blockedProviderVacancies.Any() == false)
                return null;

            return new BlockedProviderAlertViewModel
            {
                ClosedVacancies = blockedProviderVacancies.Select(v => $"{v.Title} (VAC{v.VacancyReference})").ToList(),
                BlockedProviderNames = blockedProviderVacancies.GroupBy(p => p.TrainingProviderName).Select(p => p.Key).ToList(),
            };
        }

        public TransferredVacanciesAlertViewModel GetTransferredVacanciesAlert(IEnumerable<VacancySummary> vacancies, TransferReason reason, DateTime? userLastDismissedDate)
        {
            if (userLastDismissedDate.HasValue == false)
                userLastDismissedDate = DateTime.MinValue;

            var transferredVacancyProviders = vacancies.Where(v =>
                    v.TransferInfoReason == reason &&
                    v.TransferInfoTransferredDate > userLastDismissedDate)
                .Select(v => v.TransferInfoProviderName).ToList();

            if (transferredVacancyProviders.Any() == false)
                return null;

            return new TransferredVacanciesAlertViewModel
            {
                TransferredVacanciesCount = transferredVacancyProviders.Count,
                TransferredVacanciesProviderNames = transferredVacancyProviders.GroupBy(p => p).Select(p => p.Key)
            };
        }

        public WithdrawnVacanciesAlertViewModel GetWithdrawnByQaVacanciesAlert(IEnumerable<VacancySummary> vacancies, DateTime? userLastDismissedDate)
        {
            if (userLastDismissedDate.HasValue == false)
                userLastDismissedDate = DateTime.MinValue;

            var withdrawnVacancies = GetClosedVacancies(vacancies, userLastDismissedDate, ClosureReason.WithdrawnByQa);

            if (withdrawnVacancies.Any() == false)
                return null;

            return new WithdrawnVacanciesAlertViewModel
            {
                ClosedVacancies = withdrawnVacancies.Select(v => $"{v.Title} (VAC{v.VacancyReference})").ToList()
            };
        }

        private IEnumerable<VacancySummary> GetClosedVacancies(IEnumerable<VacancySummary> vacancies, DateTime? userLastDismissedDate, ClosureReason reason)
        {
            return vacancies.Where(v =>
                v.Status == VacancyStatus.Closed &&
                v.ClosureReason == reason &&
                v.ClosedDate > userLastDismissedDate);
        }
    }
}
