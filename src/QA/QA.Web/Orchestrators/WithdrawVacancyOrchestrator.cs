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
            var vacancy = await GetVacancyAsync(m.VacancyReference.ToUpper().Replace("VAC", ""));

            if(vacancy != null && vacancy.CanClose)
                return new PostFindVacancyEditModelResult { ResultType = PostFindVacancyEditModelResultType.CanClose, VacancyReference = vacancy.VacancyReference };
            
            if(vacancy != null && vacancy.Status == VacancyStatus.Closed)
                return new PostFindVacancyEditModelResult { ResultType = PostFindVacancyEditModelResultType.AlreadyClosed, VacancyReference = vacancy.VacancyReference };

            return new PostFindVacancyEditModelResult {ResultType = PostFindVacancyEditModelResultType.NotFound};
        }

        public FindVacancyViewModel GetFindVacancyViewModel(FindVacancyEditModel m)
        {
            var vm = GetFindVacancyViewModel();

            vm.VacancyReference = m.VacancyReference;

            return vm;
        }

        public async Task<AlreadyClosedViewModel> GetAlreadyClosedViewModelAsync(string vacancyReference)
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

        public async Task<ConfirmViewModel> GetConfirmViewModelAsync(string vacancyReference)
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

        public async Task<AcknowledgeViewModel> GetAcknowledgeViewModelAsync(string vacancyReference)
        {
            var vacancy = await GetVacancyAsync(vacancyReference);

            if (vacancy == null || vacancy.CanClose == false)
                return null;

            return new AcknowledgeViewModel
            {
                VacancyReference = vacancy.VacancyReference.Value,
                OwnerName = GetOwnerName(vacancy)
            };
        }

        public async Task<bool> PostAcknowledgeEditModelAsync(AcknowledgeEditModel m, VacancyUser user)
        {
            if (m.Acknowledged == false)
                return false;

            var vacancy = await GetVacancyAsync(m.VacancyReference);

            if (vacancy == null || vacancy.CanClose == false)
                return false;

            await _vacancyClient.CloseVacancyAsync(vacancy.Id, user);

            return true;
        }

        public async Task<ClosedViewModel> GetClosedViewModelAsync(string vacancyReference)
        {
            var vacancy = await GetVacancyAsync(vacancyReference);

            if (vacancy == null || vacancy.CanClose)
                return null;

            return new ClosedViewModel
            {
                VacancyReference = vacancy.VacancyReference.Value,
                OwnerName = GetOwnerName(vacancy)
            };
        }

        private async Task<Vacancy> GetVacancyAsync(string vacancyReference)
        {
            if (long.TryParse(vacancyReference, out var vacancyReferenceNumber) == false)
                return null;

            try
            {
                return await _vacancyClient.GetVacancyAsync(vacancyReferenceNumber);
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
                    throw new InvalidEnumArgumentException($"{vacancy.OwnerType.ToString()} is not handled");
            }
        }
    }
}
