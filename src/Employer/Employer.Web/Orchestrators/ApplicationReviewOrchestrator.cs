using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings.Extensions;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Employer.Web.Exceptions;
using Esfa.Recruit.Shared.Web.Extensions;
using ApplicationReviewViewModel = Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview.ApplicationReviewViewModel;
using ApplicationStatusConfirmationViewModel = Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview.ApplicationStatusConfirmationViewModel;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public interface IApplicationReviewOrchestrator 
    {
        Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewRouteModel rm, bool vacancySharedByProvider = false);
        Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewEditModel m, bool vacancySharedByProvider = false);
        Task<ApplicationReviewStatusUpdateInfo> PostApplicationReviewConfirmationEditModelAsync(ApplicationReviewStatusConfirmationEditModel m, VacancyUser user);
        Task<ApplicationReviewCandidateInfo> PostApplicationReviewEditModelAsync(ApplicationReviewEditModel m, VacancyUser user, bool vacancySharedByProvider = false);
        Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewStatusConfirmationEditModel m);
        Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewEditModel rm);
    }

    public class ApplicationReviewOrchestrator : IApplicationReviewOrchestrator
    {
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IUtility _utility;

        public ApplicationReviewOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, IUtility utility)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _utility = utility;
        }

        public async Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewRouteModel rm, bool vacancySharedByProvider = false)
        {
            var applicationReview = await _utility.GetAuthorisedApplicationReviewAsync(rm, vacancySharedByProvider);

            var vacancy = await _vacancyClient.GetVacancyAsync(rm.VacancyId);

            if (applicationReview.IsWithdrawn)
                throw new ApplicationWithdrawnException($"Application has been withdrawn. ApplicationReviewId:{applicationReview.Id}", rm.VacancyId);

            var viewModel = applicationReview.ToViewModel();
            viewModel.EmployerAccountId = rm.EmployerAccountId;
            viewModel.VacancyId = rm.VacancyId;
            viewModel.VacancyTitle = vacancy.Title;
            viewModel.ApplicationReviewId = rm.ApplicationReviewId;
            return viewModel;
        }

        public async Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewEditModel m, bool vacancySharedByProvider = false)
        {
            var vm = await GetApplicationReviewViewModelAsync((ApplicationReviewRouteModel) m, vacancySharedByProvider);

            vm.Outcome = m.Outcome;
            vm.CandidateFeedback = m.CandidateFeedback;

            return vm;
        }

        public async Task<ApplicationReviewStatusUpdateInfo> PostApplicationReviewConfirmationEditModelAsync(ApplicationReviewStatusConfirmationEditModel m, VacancyUser user)
        {
            var applicationReview = await _utility.GetAuthorisedApplicationReviewAsync(m);

            var shouldMakeOthersUnsuccessful = await _client.SetApplicationReviewStatus(applicationReview.Id, m.Outcome, m.CandidateFeedback, user);

            var applicationReviewStatusUpdateInfo = new ApplicationReviewStatusUpdateInfo 
            {
                ShouldMakeOthersUnsuccessful = shouldMakeOthersUnsuccessful,
                CandidateName = applicationReview.Application.FullName
            };

            return applicationReviewStatusUpdateInfo;
        }

        public async Task<ApplicationReviewCandidateInfo> PostApplicationReviewEditModelAsync(ApplicationReviewEditModel m, VacancyUser user, bool vacancySharedByProvider = false)
        {
            var applicationReview = await _utility.GetAuthorisedApplicationReviewAsync(m, vacancySharedByProvider);

            await _client.SetApplicationReviewStatus(applicationReview.Id, m.Outcome, m.CandidateFeedback, user);

            return new ApplicationReviewCandidateInfo()
            {
                ApplicationReviewId = applicationReview.Id,
                FriendlyId = applicationReview.GetFriendlyId(),
                Name = applicationReview.Application.FullName
            };
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
