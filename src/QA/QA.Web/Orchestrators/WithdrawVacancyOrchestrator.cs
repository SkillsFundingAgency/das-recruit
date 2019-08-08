using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Models.WithdrawVacancy;
using Esfa.Recruit.Qa.Web.ViewModels.WithdrawVacancy;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;

namespace Esfa.Recruit.Qa.Web.Orchestrators
{
    public class WithdrawVacancyOrchestrator
    {
        private readonly IQaVacancyClient _vacancyClient;

        public WithdrawVacancyOrchestrator(IQaVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }

        public FindVacancyViewModel GetFindVacancyViewModel()
        {
            return new FindVacancyViewModel();
        }

        public async Task<PostFindVacancyEditModelResult> PostFindVacancyEditModelAsync(FindVacancyEditModel m)
        {
            var vacancyReference = m.VacancyReference.ToUpper().Replace("VAC", "");

            Vacancy vacancy = null;

            if (long.TryParse(vacancyReference, out var vacancyReferenceNumber))
                vacancy = await GetVacancyAsync(vacancyReferenceNumber);

            if(vacancy != null && vacancy.CanClose)
                return new PostFindVacancyEditModelResult { ResultType = PostFindVacancyEditModelResultType.CanClose, VacancyReference = vacancy.VacancyReference };
            
            if(vacancy != null && vacancy.Status == VacancyStatus.Closed)
                return new PostFindVacancyEditModelResult { ResultType = PostFindVacancyEditModelResultType.AlreadyClosed, VacancyReference = vacancy.VacancyReference };

            if (vacancy != null && vacancy.Status != VacancyStatus.Live)
                return new PostFindVacancyEditModelResult { ResultType = PostFindVacancyEditModelResultType.CannotClose, VacancyReference = vacancy.VacancyReference };

            return new PostFindVacancyEditModelResult {ResultType = PostFindVacancyEditModelResultType.NotFound};
        }

        public FindVacancyViewModel GetFindVacancyViewModel(FindVacancyEditModel m)
        {
            var vm = GetFindVacancyViewModel();

            vm.VacancyReference = m.VacancyReference;

            return vm;
        }

        public async Task<AlreadyClosedViewModel> GetAlreadyClosedViewModelAsync(long vacancyReference)
        {
            var vacancy = await GetVacancyAsync(vacancyReference);

            if (vacancy == null || vacancy.CanClose)
                return null;

            return new AlreadyClosedViewModel
            {
                VacancyReference = vacancy.VacancyReference.Value,
                Title = vacancy.Title,
                LegalEntityName = vacancy.LegalEntityName,
                ClosedDate = vacancy.ClosedDate.Value.AsGdsDate()
            };
        }

        public async Task<ConfirmViewModel> GetConfirmViewModelAsync(long vacancyReference)
        {
            var vacancy = await GetVacancyAsync(vacancyReference);

            if (vacancy == null || vacancy.CanClose == false)
                return null;

            return new ConfirmViewModel
            {
                VacancyReference = vacancy.VacancyReference.Value,
                Title = vacancy.Title,
                Status = vacancy.Status.ToString(),
                TrainingProvider = vacancy.TrainingProvider.Name,
                LegalEntityName = vacancy.LegalEntityName,
                Owner = vacancy.OwnerType
            };
        }

        public async Task<ConsentViewModel> GetConsentViewModelAsync(long vacancyReference)
        {
            var vacancy = await GetVacancyAsync(vacancyReference);

            if (vacancy == null || vacancy.CanClose == false)
                return null;

            return new ConsentViewModel
            {
                VacancyReference = vacancy.VacancyReference.Value,
                OwnerName = GetOwnerName(vacancy)
            };
        }

        public async Task<bool> PostConsentEditModelAsync(ConsentEditModel m, long vacancyReference, VacancyUser user)
        {
            if (m.Acknowledged == false)
                return false;

            var vacancy = await GetVacancyAsync(vacancyReference);

            if (vacancy == null || vacancy.CanClose == false)
                return false;

            await _vacancyClient.CloseVacancyAsync(vacancy.Id, user);

            return true;
        }

        public async Task<AcknowledgementViewModel> GetAcknowledgementViewModelAsync(long vacancyReference)
        {
            var vacancy = await GetVacancyAsync(vacancyReference);

            if (vacancy == null || vacancy.CanClose)
                return null;

            return new AcknowledgementViewModel
            {
                VacancyReference = vacancy.VacancyReference.Value,
                OwnerName = GetOwnerName(vacancy)
            };
        }

        private async Task<Vacancy> GetVacancyAsync(long vacancyReference)
        {
            try
            {
                return await _vacancyClient.GetVacancyAsync(vacancyReference);
            }
            catch (VacancyNotFoundException)
            {
                return null;
            }
        }

        private string GetOwnerName(Vacancy vacancy)
        {
            switch (vacancy.OwnerType)
            {
                case OwnerType.Employer:
                    return vacancy.LegalEntityName;
                case OwnerType.Provider:
                    return vacancy.TrainingProvider.Name;
                default:
                    throw new NotImplementedException($"{vacancy.OwnerType.ToString()} is not handled");
            }
        }
    }
}
