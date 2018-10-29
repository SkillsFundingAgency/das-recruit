﻿using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
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

        public async Task<LevySelectionOrchestratorResponse> SaveSelectionAsync(LevyDeclarationModel viewModel, ClaimsPrincipal user)
        {
            await _client.SaveLevyDeclarationAsync(viewModel.ConfirmAsLevyPayer.Value, user.GetUserId());

            return new LevySelectionOrchestratorResponse 
            {
                RedirectRouteName = viewModel.ConfirmAsLevyPayer.Value ? RouteNames.Dashboard_Index_Get : RouteNames.NonLevyInfo_Get,
                CreateLevyCookie = viewModel.ConfirmAsLevyPayer.Value
            };
        }
    }

    public class LevySelectionOrchestratorResponse
    {
        public string RedirectRouteName { get; set; }

        public bool CreateLevyCookie { get; set; }
    }

}