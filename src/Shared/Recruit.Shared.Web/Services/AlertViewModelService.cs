﻿using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels.Alerts;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Shared.Web.Services
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

            var blockedProviderNames = blockedProviderVacancies
                .Select(v => v.TrainingProviderName)
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            return new BlockedProviderAlertViewModel
            {
                ClosedVacancies = blockedProviderVacancies.Select(v => $"{v.Title} (VAC{v.VacancyReference})").ToList(),
                BlockedProviderNames = blockedProviderNames
            };
        }

        public EmployerTransferredVacanciesAlertViewModel GetTransferredVacanciesAlert(IEnumerable<VacancySummary> vacancies, TransferReason reason, DateTime? userLastDismissedDate)
        {
            if (userLastDismissedDate.HasValue == false)
                userLastDismissedDate = DateTime.MinValue;

            var transferredVacancies = vacancies.Where(v =>
                    v.TransferInfoReason == reason &&
                    v.TransferInfoTransferredDate > userLastDismissedDate)
                .ToList();

            if (transferredVacancies.Any() == false)
                return null;

            var transferredVacanciesProviderNames = transferredVacancies
                .Select(v => v.TransferInfoProviderName)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            return new EmployerTransferredVacanciesAlertViewModel
            {
                TransferredVacanciesCount = transferredVacancies.Count,
                TransferredVacanciesProviderNames = transferredVacanciesProviderNames
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
                ClosedVacancies = withdrawnVacancies.Select(v => $"{v.Title} (VAC{v.VacancyReference})").ToList(),
            };
        }

        public ProviderTransferredVacanciesAlertViewModel GetProviderTransferredVacanciesAlert(IEnumerable<ProviderDashboardTransferredVacancy> transferredVacancies, DateTime? userLastDismissedDate)
        {
            if (transferredVacancies == null)
                return null;

            if (userLastDismissedDate.HasValue == false)
                userLastDismissedDate = DateTime.MinValue;

            var usersTransferredVacancies = transferredVacancies
                .Where(t => t.TransferredDate > userLastDismissedDate)
                .Select(t => t.LegalEntityName)
                .ToList();

            if (usersTransferredVacancies.Any() == false)
                return null;

            return new ProviderTransferredVacanciesAlertViewModel
            {
                LegalEntityNames = usersTransferredVacancies.Distinct().ToList()
            };
        }

        private IList<VacancySummary> GetClosedVacancies(IEnumerable<VacancySummary> vacancies, DateTime? userLastDismissedDate, ClosureReason reason)
        {
            return vacancies.Where(v =>
                    v.Status == VacancyStatus.Closed &&
                    v.ClosureReason == reason &&
                    v.ClosedDate > userLastDismissedDate)
                .OrderBy(v => v.Title)
                .ToList();
        }
    }
}
