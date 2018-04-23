﻿using Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class DeleteVacancyOrchestrator
    {
        private readonly IEmployerVacancyClient _client;

        public DeleteVacancyOrchestrator(IEmployerVacancyClient client)
        {
            _client = client;
        }

        public async Task<DeleteViewModel> GetDeleteViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            Utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            if (!vacancy.CanDelete)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new DeleteViewModel
            {
                Title = vacancy.Title
            };

            return vm;
        }

        public async Task<bool> TryDeleteVacancyAsync(DeleteEditModel m, VacancyUser user)
        {
            var vacancy = await _client.GetVacancyAsync(m.VacancyId);

            Utility.CheckAuthorisedAccess(vacancy, m.EmployerAccountId);

            if (!vacancy.CanDelete)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            return await _client.DeleteVacancyAsync(m.VacancyId, user);
        }
    }
}
