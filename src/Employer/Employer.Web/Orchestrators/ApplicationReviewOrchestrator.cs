using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings.Extensions;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Employer.Web.Exceptions;
using ApplicationReviewViewModel = Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview.ApplicationReviewViewModel;
using ApplicationStatusConfirmationViewModel = Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview.ApplicationStatusConfirmationViewModel;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class ApplicationReviewOrchestrator
    {
        private readonly IEmployerVacancyClient _client;
        private readonly IUtility _utility;

        public ApplicationReviewOrchestrator(IEmployerVacancyClient client, IUtility utility)
        {
            _client = client;
            _utility = utility;
        }

        public async Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewRouteModel rm)
        {
            var vacancy = await _client.GetVacancyAsync(rm.VacancyId);
            var applicationReview = await _utility.GetAuthorisedApplicationReviewAsync(rm);
           
            if (applicationReview.IsWithdrawn)
                throw new ApplicationWithdrawnException($"Application has been withdrawn. ApplicationReviewId:{applicationReview.Id}", rm.VacancyId);

            var viewModel = applicationReview.ToViewModel(vacancy);
            viewModel.EmployerAccountId = rm.EmployerAccountId;
            viewModel.VacancyId = rm.VacancyId;
            viewModel.ApplicationReviewId = rm.ApplicationReviewId;
            return viewModel;
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
            var applicationReview = await _utility.GetAuthorisedApplicationReviewAsync(m);

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

        public async Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewStatusConfirmationEditModel m)
        {            
            var applicationReview = await _utility.GetAuthorisedApplicationReviewAsync(m);

            return new ApplicationStatusConfirmationViewModel {
                EmployerAccountId = m.EmployerAccountId,
                VacancyId = m.VacancyId,
                ApplicationReviewId = m.ApplicationReviewId,
                
                CandidateFeedback = m.CandidateFeedback,
                Outcome = m.Outcome,
                Name = applicationReview.Application.FullName
            };
        }

        public async Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewEditModel rm)
        {
            var applicationReviewVm = await GetApplicationReviewViewModelAsync((ApplicationReviewRouteModel) rm);
            
            return new ApplicationStatusConfirmationViewModel {                
                CandidateFeedback = rm.CandidateFeedback,                
                Outcome = rm.Outcome,
                Name = applicationReviewVm.Name                
            };
        }
    }
}
