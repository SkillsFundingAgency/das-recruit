using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Submitted;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Employer.Web.RouteModel;
using System;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class SubmittedOrchestrator
    {
        private readonly IEmployerVacancyClient _client;

        public SubmittedOrchestrator(IEmployerVacancyClient client)
        {
            _client = client;
        }

        public async Task<VacancySubmittedConfirmationViewModel> GetVacancySubmittedConfirmationViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyAsync(async (Guid id) => await _client.GetVacancyAsync(id), vrm, RouteNames.Submitted_Index_Get);

            if (vacancy.Status != VacancyStatus.Submitted)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotSubmittedSuccessfully, vacancy.Title));

            var vm = new VacancySubmittedConfirmationViewModel
            {
                Title = vacancy.Title,
                VacancyReference = vacancy.VacancyReference?.ToString()
            };

            return vm;
        }
    }
}