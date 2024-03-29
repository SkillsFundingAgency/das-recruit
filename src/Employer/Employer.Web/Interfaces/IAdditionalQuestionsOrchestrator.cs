﻿using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.AdditionalQuestions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Interfaces;

public interface IAdditionalQuestionsOrchestrator
{
    Task<AdditionalQuestionsViewModel> GetViewModel(VacancyRouteModel routeModel);
    Task<OrchestratorResponse> PostEditModel(AdditionalQuestionsEditModel editModel, VacancyUser user);
}