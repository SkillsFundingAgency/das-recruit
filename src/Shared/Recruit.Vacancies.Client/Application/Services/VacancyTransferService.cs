using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class VacancyTransferService : IVacancyTransferService
    {
        private readonly ITimeProvider _timeProvider;

        public VacancyTransferService(ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public Task TransferVacancyToLegalEntityAsync(Vacancy vacancy, VacancyUser initiatingUser, bool isProviderBlocked)
        {
            var originalStatus = vacancy.Status;

            switch (originalStatus)
            {
                case VacancyStatus.Draft:
                case VacancyStatus.Referred:
                case VacancyStatus.Closed:
                    break;
                case VacancyStatus.Submitted:
                    vacancy.Status = VacancyStatus.Draft;
                    break;
                case VacancyStatus.Live:
                    CloseVacancy(vacancy, initiatingUser);
                    break;
                case VacancyStatus.Approved:
                    vacancy.ApprovedDate = null;
                    CloseVacancy(vacancy, initiatingUser);
                    break;
                default:
                    throw new ArgumentException(string.Format(ExceptionMessages.UnrecognisedStatusToTransferVacancyFrom, originalStatus.ToString(), vacancy.VacancyReference));
            }

            vacancy.TransferInfo = new TransferInfo
            {
                Ukprn = vacancy.TrainingProvider.Ukprn.GetValueOrDefault(),
                ProviderName = vacancy.TrainingProvider.Name,
                LegalEntityName = vacancy.LegalEntityName,
                TransferredByUser = initiatingUser,
                TransferredDate = _timeProvider.Today,
                Reason = isProviderBlocked ? TransferReason.BlockedByQa : TransferReason.EmployerRevokedProviderPermission
            };

            vacancy.OwnerType = OwnerType.Employer;
            vacancy.ProviderContact = null;
            vacancy.SubmittedByUser = null;

            return Task.CompletedTask;
        }

        private void CloseVacancy(Vacancy vacancy, VacancyUser initiatingUser)
        {
            vacancy.Status = VacancyStatus.Closed;
            vacancy.ClosedDate = _timeProvider.Now;
            vacancy.ClosedByUser = initiatingUser;
            vacancy.ClosureReason = ClosureReason.TransferredByEmployer;
        }
    }
}