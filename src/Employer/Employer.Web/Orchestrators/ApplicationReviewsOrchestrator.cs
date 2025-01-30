using System.Linq;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public interface IApplicationReviewsOrchestrator
    {
        Task<ApplicationReviewsToUnsuccessfulViewModel> GetApplicationReviewsToUnsuccessfulViewModelAsync(VacancyRouteModel rm, SortColumn sortColumn, SortOrder sortOrder);
        Task<ApplicationReviewsToUnsuccessfulConfirmationViewModel> GetApplicationReviewsToUnsuccessfulConfirmationViewModelAsync(ApplicationReviewsToUnsuccessfulRouteModel rm);
        Task<ApplicationReviewsFeedbackViewModel> GetApplicationReviewsFeedbackViewModel(ApplicationReviewsToUnsuccessfulRouteModel rm);
        Task PostApplicationReviewsToUnsuccessfulAsync(ApplicationReviewsToUnsuccessfulConfirmationViewModel request, VacancyUser user);
        Task PostApplicationReviewsStatus(ApplicationReviewsToUpdateStatusModel request, VacancyUser user, ApplicationReviewStatus? applicationReviewStatus, ApplicationReviewStatus? applicationReviewTemporaryStatus);
        Task PostApplicationReviewPendingUnsuccessfulFeedback(ApplicationReviewStatusModel request, VacancyUser user, ApplicationReviewStatus applicationReviewStatus);
    }

    public class ApplicationReviewsOrchestrator : IApplicationReviewsOrchestrator
    {
        private readonly IRecruitVacancyClient _vacancyClient;

        public ApplicationReviewsOrchestrator(IRecruitVacancyClient client)
        {
            _vacancyClient = client;
        }

        public async Task<ApplicationReviewsToUnsuccessfulViewModel> GetApplicationReviewsToUnsuccessfulViewModelAsync(VacancyRouteModel rm, SortColumn sortColumn, SortOrder sortOrder)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(rm.VacancyId);

            var applicationReviews = await _vacancyClient.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference!.Value, sortColumn, sortOrder);
            var applicationsSelected =
                await _vacancyClient.GetVacancyApplicationsForReferenceAndStatus(rm.VacancyId,
                    ApplicationReviewStatus.PendingToMakeUnsuccessful);

            var vacancyApplications = applicationReviews.Where(fil => fil.IsNotWithdrawn).ToList();
            foreach (var application in applicationsSelected)
            {
                if (vacancyApplications.FirstOrDefault(c => c.ApplicationId == application.ApplicationId) != null)
                {
                    vacancyApplications.FirstOrDefault(c => c.ApplicationId == application.ApplicationId)!.Selected = true;    
                }
            }

            return new ApplicationReviewsToUnsuccessfulViewModel
            {
                VacancyId = vacancy.Id,
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyApplications = vacancyApplications
            };
        }
        
        public async Task<ApplicationReviewsFeedbackViewModel> GetApplicationReviewsFeedbackViewModel(ApplicationReviewsToUnsuccessfulRouteModel rm)
        {
            var applicationsToUnsuccessful =
                await _vacancyClient.GetVacancyApplicationsForReferenceAndStatus(rm.VacancyId!,
                    ApplicationReviewStatus.PendingToMakeUnsuccessful);
            return new ApplicationReviewsFeedbackViewModel
            {
                VacancyId = rm.VacancyId,
                EmployerAccountId = rm.EmployerAccountId,
                ApplicationsToUnsuccessful = applicationsToUnsuccessful,
                Outcome = ApplicationReviewStatus.Unsuccessful
            };
        }

        public async Task<ApplicationReviewsToUnsuccessfulConfirmationViewModel> GetApplicationReviewsToUnsuccessfulConfirmationViewModelAsync(ApplicationReviewsToUnsuccessfulRouteModel rm)
        {
            //TODO FAI-2258 dont use this.
            var applicationsToUnsuccessful =
                await _vacancyClient.GetVacancyApplicationsForReferenceAndStatus(rm.VacancyId,
                    ApplicationReviewStatus.PendingToMakeUnsuccessful);

            return new ApplicationReviewsToUnsuccessfulConfirmationViewModel
            {
                VacancyId = rm.VacancyId,
                EmployerAccountId = rm.EmployerAccountId,
                VacancyApplicationsToUnsuccessful = applicationsToUnsuccessful,
                CandidateFeedback = applicationsToUnsuccessful.FirstOrDefault()!.CandidateFeedback
            };
        }
        public async Task PostApplicationReviewsStatus(ApplicationReviewsToUpdateStatusModel request, VacancyUser user, ApplicationReviewStatus? applicationReviewStatus, ApplicationReviewStatus? applicationReviewTemporaryStatus)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(request.VacancyId);
            await _vacancyClient.SetApplicationReviewsStatus(vacancy!.VacancyReference!.Value, request.ApplicationReviewIds, user, applicationReviewStatus, request.VacancyId,applicationReviewTemporaryStatus);
        }
        
        public async Task PostApplicationReviewPendingUnsuccessfulFeedback(ApplicationReviewStatusModel request, VacancyUser user, ApplicationReviewStatus applicationReviewStatus)
        {
            await _vacancyClient.SetApplicationReviewsPendingUnsuccessfulFeedback(user, applicationReviewStatus, request.VacancyId, request.CandidateFeedback);
        }
        
        public async Task PostApplicationReviewsToUnsuccessfulAsync(ApplicationReviewsToUnsuccessfulConfirmationViewModel request, VacancyUser user)
        {
            //TODO FAI-2258 dont use this.
            await _vacancyClient.SetApplicationReviewsToUnsuccessful(request.VacancyApplicationsToUnsuccessful.Select(c=>c.ApplicationReviewId), request.CandidateFeedback, user, request.VacancyId);
        }
    }
}
