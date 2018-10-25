using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels.LevyDeclaration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class LevyDeclarationOrchestrator
    {
        private readonly IEmployerVacancyClient _client;

        public LevyDeclarationOrchestrator(IEmployerVacancyClient client)
        {
            _client = client;
        }

        public Task SaveSelection(LevyDeclarationModel viewModel, string userId)
        {
            return _client.SaveLevyDeclarationAsync(viewModel.ConfirmAsLevyPayer.Value, userId);
        }
    }
}