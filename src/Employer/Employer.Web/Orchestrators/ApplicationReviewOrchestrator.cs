﻿using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings.Extensions;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class ApplicationReviewOrchestrator
    {
        private readonly IEmployerVacancyClient _client;

        public ApplicationReviewOrchestrator(IEmployerVacancyClient client)
        {
            _client = client;
        }

        public async Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewRouteModel rm)
        {
            var applicationReview = await Utility.GetAuthorisedApplicationReviewAsync(_client, rm);

            if (applicationReview.IsWithdrawn)
                throw new Exception($"Application has been withdrawn. ApplicationReviewId:{applicationReview.Id}");

            return applicationReview.ToViewModel();
        }

        public async Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewEditModel m)
        {
            var vm = await GetApplicationReviewViewModelAsync((ApplicationReviewRouteModel) m);

            vm.Outcome = m.Outcome;
            vm.CandidateFeedback = m.CandidateFeedback;

            return vm;
        }

        public Task PostApplicationReviewEditModelAsync(ApplicationReviewEditModel m, VacancyUser user)
        {
            switch (m.Outcome.Value)
            {
                case ApplicationReviewStatus.Successful:
                    return _client.SetApplicationReviewSuccessful(m.ApplicationReviewId, user);
                case ApplicationReviewStatus.Unsuccessful:
                    return _client.SetApplicationReviewUnsuccessful(m.ApplicationReviewId, m.CandidateFeedback, user);
                default:
                    throw new ArgumentException("Unhandled ApplicationReviewStatus");
            }
        }
    }
}
