﻿using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class CloseVacancyOrchestrator
    {
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;

        public CloseVacancyOrchestrator(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient)
        {
            _client = client;
            _vacancyClient = vacancyClient;
        }

        public async Task<CloseViewModel> GetCloseViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(vrm.VacancyId.GetValueOrDefault());

            Utility.CheckAuthorisedAccess(vacancy, vrm.Ukprn);

            if (!vacancy.CanClose)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForClosing, vacancy.Title));

            var vm = new CloseViewModel {
                Title = vacancy.Title,
                VacancyReference = vacancy.VacancyReference.Value.ToString()
            };

            return vm;
        }

        public async Task<OrchestratorResponse<VacancyInfo>> CloseVacancyAsync(CloseEditModel m, VacancyUser user)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(m.VacancyId.GetValueOrDefault());

            Utility.CheckAuthorisedAccess(vacancy, m.Ukprn);

            if (!vacancy.CanClose)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForClosing, vacancy.Title));

            await _vacancyClient.CloseVacancyAsync(vacancy.Id, user, ClosureReason.Manual);

            return new OrchestratorResponse<VacancyInfo>(new VacancyInfo {
                Id = vacancy.Id,
                VacancyReference = vacancy.VacancyReference.Value.ToString(),
                Title = vacancy.Title
            });
        }
    }
}
