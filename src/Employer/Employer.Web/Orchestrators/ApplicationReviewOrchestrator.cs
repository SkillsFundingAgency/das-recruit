using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings.Extensions;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Employer.Web.Exceptions;

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
                throw new ApplicationWithdrawnException($"Application has been withdrawn. ApplicationReviewId:{applicationReview.Id}", rm.VacancyId);

            return applicationReview.ToViewModel();
        }

        public async Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewEditModel m)
        {
            var vm = await GetApplicationReviewViewModelAsync((ApplicationReviewRouteModel) m);

            vm.Outcome = m.Outcome;
            vm.CandidateFeedback = m.CandidateFeedback;

            return vm;
        }

        public async Task<string> PostApplicationReviewConfirmationEditModelAsync(ApplicationReviewStatusConfirmationEditModel m, VacancyUser user)
        {
            var applicationReview = await Utility.GetAuthorisedApplicationReviewAsync(_vacancyClient, m);

            switch (m.Outcome.Value)
            {
                case ApplicationReviewStatus.Successful:
                    await _client.SetApplicationReviewSuccessful(applicationReview.Id, user);
                    break;
                case ApplicationReviewStatus.Unsuccessful:
                    await _client.SetApplicationReviewUnsuccessful(applicationReview.Id, m.CandidateFeedback, user);
                    break;
                default:
                    throw new ArgumentException("Unhandled ApplicationReviewStatus");
            }
            return applicationReview.Application.FullName;
        }                

        internal async Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewStatusConfirmationEditModel m)
        {            
            await Utility.GetAuthorisedApplicationReviewAsync(_vacancyClient, m);

            var applicationReview = await Utility.GetAuthorisedApplicationReviewAsync(_vacancyClient, m);

            return new ApplicationStatusConfirmationViewModel {
                CandidateFeedback = m.CandidateFeedback,
                Outcome = m.Outcome,
                ApplicationReviewId = m.ApplicationReviewId,
                Name = applicationReview.Application.FullName
            };
        }

        public async Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewEditModel rm)
        {
            var applicationReviewVm = await GetApplicationReviewViewModelAsync((ApplicationReviewRouteModel) rm);
            
            return new ApplicationStatusConfirmationViewModel {                
                CandidateFeedback = rm.CandidateFeedback,                
                Outcome = rm.Outcome,
                ApplicationReviewId = rm.ApplicationReviewId,
                Name = applicationReviewVm.Name                
            };
        }        
    }
}
