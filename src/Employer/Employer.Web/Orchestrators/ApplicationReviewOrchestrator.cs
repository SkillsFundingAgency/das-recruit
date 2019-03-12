﻿using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings.Extensions;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class ApplicationReviewOrchestrator
    {
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;

        public ApplicationReviewOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient)
        {
            _client = client;
            _vacancyClient = vacancyClient;
        }

        public async Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewRouteModel rm)
        {
            var applicationReview = await Utility.GetAuthorisedApplicationReviewAsync(_vacancyClient, rm);

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

        public Task PostApplicationReviewEditModelAsync(ApplicationReviewStatusConfirmationEditModel m, VacancyUser user)
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

        internal async Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewStatusConfirmationEditModel m)
        {            
            await Utility.GetAuthorisedApplicationReviewAsync(_vacancyClient, m);

            return new ApplicationStatusConfirmationViewModel {
                CandidateFeedback = m.CandidateFeedback,
                Outcome = m.Outcome,
                ApplicationReviewId = m.ApplicationReviewId
            };
        }

        public async Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewEditModel rm)
        {
            var applicationReviewVm = await GetApplicationReviewViewModelAsync((ApplicationReviewRouteModel) rm);
            
            return new ApplicationStatusConfirmationViewModel {                
                CandidateFeedback = rm.CandidateFeedback,                
                Outcome = rm.Outcome,
                ApplicationReviewId = rm.ApplicationReviewId,
                Name= applicationReviewVm.Name,
                Email = applicationReviewVm.Email
            };
        }
    }
}
