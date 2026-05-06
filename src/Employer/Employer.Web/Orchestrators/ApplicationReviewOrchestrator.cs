using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Exceptions;
using Esfa.Recruit.Employer.Web.Mappings.Extensions;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using ApplicationReviewViewModel = Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview.ApplicationReviewViewModel;
using ApplicationStatusConfirmationViewModel = Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview.ApplicationStatusConfirmationViewModel;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public interface IApplicationReviewOrchestrator 
    {
        Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewRouteModel rm);
        Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewEditModel m);
        Task<ApplicationReviewStatusUpdateInfo> PostApplicationReviewConfirmationEditModelAsync(ApplicationReviewStatusConfirmationEditModel m, VacancyUser user);
        Task<ApplicationReviewCandidateInfo> PostApplicationReviewEditModelAsync(ApplicationReviewEditModel m, VacancyUser user);
        Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewStatusConfirmationEditModel m);
        Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewEditModel rm);
        Task<bool> IsAllApplicationReviewsHasOutcomeAsync(Guid vacancyId);
    }

    public class ApplicationReviewOrchestrator(
        IEmployerVacancyClient client,
        IRecruitVacancyClient vacancyClient,
        IUtility utility)
        : IApplicationReviewOrchestrator
    {
        public async Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewRouteModel rm)
        {
            var applicationReview = await utility.GetAuthorisedApplicationReviewAsync(rm);

            var vacancy = await vacancyClient.GetVacancyAsync(rm.VacancyId);

            if (applicationReview.IsWithdrawn)
                throw new ApplicationWithdrawnException($"Application has been withdrawn. ApplicationReviewId:{applicationReview.Id}", rm.VacancyId);

            var viewModel = applicationReview.ToViewModel();
            viewModel.EmployerAccountId = rm.EmployerAccountId;
            viewModel.VacancyId = rm.VacancyId;
            viewModel.VacancyTitle = vacancy.Title;
            viewModel.ApplicationReviewId = rm.ApplicationReviewId;
            viewModel.IsFoundation = vacancy.ApprenticeshipType == ApprenticeshipTypes.Foundation;
            viewModel.CandidateAppliedLocations = applicationReview.Application is not null ? applicationReview.Application.CandidateAppliedLocations : string.Empty;
            return viewModel;
        }

        public async Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewEditModel m)
        {
            var vm = await GetApplicationReviewViewModelAsync((ApplicationReviewRouteModel) m);

            vm.Outcome = m.Outcome;
            vm.CandidateFeedback = m.CandidateFeedback;

            return vm;
        }

        public async Task<ApplicationReviewStatusUpdateInfo> PostApplicationReviewConfirmationEditModelAsync(ApplicationReviewStatusConfirmationEditModel m, VacancyUser user)
        {
            var applicationReview = await utility.GetAuthorisedApplicationReviewAsync(m);

            var shouldMakeOthersUnsuccessful = await client.SetApplicationReviewStatus(applicationReview.Id, m.Outcome, m.CandidateFeedback, user);

            var applicationReviewStatusUpdateInfo = new ApplicationReviewStatusUpdateInfo 
            {
                ShouldMakeOthersUnsuccessful = shouldMakeOthersUnsuccessful,
                CandidateName = applicationReview.Application.FullName
            };

            return applicationReviewStatusUpdateInfo;
        }

        public async Task<ApplicationReviewCandidateInfo> PostApplicationReviewEditModelAsync(ApplicationReviewEditModel m, VacancyUser user)
        {
            var applicationReview = await utility.GetAuthorisedApplicationReviewAsync(m);

            await client.SetApplicationReviewStatus(applicationReview.Id, m.Outcome, m.CandidateFeedback, user);

            return new ApplicationReviewCandidateInfo()
            {
                ApplicationReviewId = applicationReview.Id,
                FriendlyId = applicationReview.GetFriendlyId(),
                Name = applicationReview.Application.FullName
            };
        }

        public async Task<ApplicationStatusConfirmationViewModel> GetApplicationStatusConfirmationViewModelAsync(ApplicationReviewStatusConfirmationEditModel m)
        {            
            var applicationReview = await utility.GetAuthorisedApplicationReviewAsync(m);

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
            
            return new ApplicationStatusConfirmationViewModel
            {
                CandidateFeedback = rm.CandidateFeedback,
                Outcome = rm.Outcome,
                Name = applicationReviewVm.Name,
                FriendlyId = applicationReviewVm.FriendlyId
            };
        }

        public async Task<bool> IsAllApplicationReviewsHasOutcomeAsync(Guid vacancyId)
        {
            var applicationReviews = await vacancyClient.GetApplicationReviewsAsync(vacancyId);
            return applicationReviews
                .Where(ar => !ar.IsWithdrawn)
                .All(ar => ar.Status is ApplicationReviewStatus.Successful or ApplicationReviewStatus.Unsuccessful);
        }
    }
}
