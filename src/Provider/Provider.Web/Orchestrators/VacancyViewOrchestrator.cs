using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyView;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacancyViewOrchestrator
    {
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;
        private readonly IRecruitVacancyClient _client;
        private readonly IUtility _utility;

        public VacancyViewOrchestrator(DisplayVacancyViewModelMapper vacancyDisplayMapper, IRecruitVacancyClient client, IUtility utility)
        {
            _vacancyDisplayMapper = vacancyDisplayMapper;
            _client = client;
            _utility = utility;
        }

        public async Task<Vacancy> GetVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId.GetValueOrDefault());

            _utility.CheckAuthorisedAccess(vacancy, vrm.Ukprn);

            return vacancy;
        }

        /// <summary>
        /// Gets vacancy for display without applications.
        /// </summary>
        /// <param name="vacancy"></param>
        /// <returns></returns>
        public async Task<DisplayVacancyViewModel> GetFullVacancyDisplayViewModelAsync(Vacancy vacancy)
        {
            switch (vacancy.Status)
            {
                case VacancyStatus.Approved:
                    var approvedViewModel = new ApprovedVacancyViewModel();
                    await _vacancyDisplayMapper.MapFromVacancyAsync(approvedViewModel, vacancy);
                    approvedViewModel.ApprovedDate = vacancy.ApprovedDate.Value.AsGdsDate();
                    return approvedViewModel;
                case VacancyStatus.Live:
                    var liveViewModel = new LiveVacancyViewModel();
                    await _vacancyDisplayMapper.MapFromVacancyAsync(liveViewModel, vacancy);
                    return liveViewModel;
                case VacancyStatus.Closed:
                    var closedViewModel = new ClosedVacancyViewModel();
                    await _vacancyDisplayMapper.MapFromVacancyAsync(closedViewModel, vacancy);
                    return closedViewModel;
                case VacancyStatus.Review:
                    var reviewViewModel = new ReviewVacancyViewModel();
                    await _vacancyDisplayMapper.MapFromVacancyAsync(reviewViewModel, vacancy);
                    return reviewViewModel;
                case VacancyStatus.Submitted:
                    var submittedViewModel = new SubmittedVacancyViewModel();
                    await _vacancyDisplayMapper.MapFromVacancyAsync(submittedViewModel, vacancy);
                    submittedViewModel.SubmittedDate = vacancy.SubmittedDate.Value.AsGdsDate();
                    return submittedViewModel;
                default:
                    throw new InvalidStateException(string.Format(ErrorMessages.VacancyCannotBeViewed, vacancy.Title));
            }
        }

        public async Task<ViewVacancy> GetVacancyDisplayViewModelAsync(Vacancy vacancy)
        {
            switch (vacancy.Status)
            {
                case VacancyStatus.Submitted:
                    return await GetDisplayViewModelForSubmittedVacancy(vacancy);
                case VacancyStatus.Review:
                    return await GetDisplayViewModelForReviewVacancy(vacancy);
                case VacancyStatus.Approved:
                    return await GetDisplayViewModelForApprovedVacancy(vacancy);
                case VacancyStatus.Live:
                    return GetDisplayViewModelForLiveVacancy(vacancy);
                case VacancyStatus.Closed:
                    return GetDisplayViewModelForClosedVacancy(vacancy);
                case VacancyStatus.Referred:
                    return await GetDisplayViewModelForReferredVacancy(vacancy);
                case VacancyStatus.Rejected:
                    return await GetDisplayViewModelForRejectedVacancy(vacancy);
                default:
                    throw new InvalidStateException(string.Format(ErrorMessages.VacancyCannotBeViewed, vacancy.Title));
            }
        }

        private async Task<ViewVacancy> GetDisplayViewModelForSubmittedVacancy(Vacancy vacancy)
        {
            var submittedViewModel = new SubmittedVacancyViewModel();
            await _vacancyDisplayMapper.MapFromVacancyAsync(submittedViewModel, vacancy);
            submittedViewModel.SubmittedDate = vacancy.SubmittedDate.Value.AsGdsDate();
            return new ViewVacancy
            {
                ViewModel = submittedViewModel,
                ViewName = ViewNames.ManageSubmittedVacancyView
            };
        }

        private async Task<ViewVacancy> GetDisplayViewModelForReviewVacancy(Vacancy vacancy)
        {
            var reviewViewModel = new ReviewVacancyViewModel();
            await _vacancyDisplayMapper.MapFromVacancyAsync(reviewViewModel, vacancy);
            reviewViewModel.ReviewDate = vacancy.ReviewDate.Value.AsGdsDate();
            return new ViewVacancy
            {
                ViewModel = reviewViewModel,
                ViewName = ViewNames.ManageReviewVacancyView
            };
        }

        private async Task<ViewVacancy> GetDisplayViewModelForApprovedVacancy(Vacancy vacancy)
        {
            var approvedViewModel = new ApprovedVacancyViewModel();
            await _vacancyDisplayMapper.MapFromVacancyAsync(approvedViewModel, vacancy);
            approvedViewModel.ApprovedDate = vacancy.ApprovedDate.Value.AsGdsDate();
            return new ViewVacancy
            {
                ViewModel = approvedViewModel,
                ViewName = ViewNames.ManageApprovedVacancyView
            };
        }

        private ViewVacancy GetDisplayViewModelForLiveVacancy(Vacancy vacancy)
        {
            var liveViewModel = new LiveVacancyViewModel();
            PopulateViewModelWithApplications(vacancy, liveViewModel);
            return new ViewVacancy
            {
                ViewModel = liveViewModel,
                ViewName = liveViewModel.HasApplications ? ViewNames.ManageLiveVacancyWithApplicationsView : ViewNames.ManageLiveVacancyView
            };
        }

        private ViewVacancy GetDisplayViewModelForClosedVacancy(Vacancy vacancy)
        {
            var closedViewModel = new ClosedVacancyViewModel();
            PopulateViewModelWithApplications(vacancy, closedViewModel);
            closedViewModel.ClosedDate = vacancy.ClosedDate.Value.AsGdsDate();
            return new ViewVacancy
            {
                ViewModel = closedViewModel,
                ViewName = closedViewModel.HasApplications ? ViewNames.ManageClosedVacancyWithApplicationsView : ViewNames.ManageClosedVacancyView
            };
        }

        private void PopulateViewModelWithApplications(Vacancy vacancy, DisplayVacancyApplicationViewModel viewModel)
        {
            var mappedDisplayVacancyViewModelTask = _vacancyDisplayMapper.MapFromVacancyAsync(viewModel, vacancy);
            var vacancyApplicationsTask = _client.GetVacancyApplicationsAsync(vacancy.VacancyReference.Value);

            Task.WaitAll(mappedDisplayVacancyViewModelTask, vacancyApplicationsTask);

            var applications = vacancyApplicationsTask.Result ?? new List<VacancyApplication>();

            viewModel.Applications = new VacancyApplicationsViewModel
            {
                Applications = applications,
                ShowDisability = vacancy.IsDisabilityConfident,
                Ukprn = viewModel.Ukprn,
                VacancyId = viewModel.VacancyId
            };
        }

        private async Task<ViewVacancy> GetDisplayViewModelForReferredVacancy(Vacancy vacancy)
        {
            var referredViewModel = new ReferredVacancyViewModel();
            await _vacancyDisplayMapper.MapFromVacancyAsync(referredViewModel, vacancy);
            return new ViewVacancy
            {
                ViewModel = referredViewModel,
                ViewName = ViewNames.ManageReferredVacancyView
            };
        }

        private async Task<ViewVacancy> GetDisplayViewModelForRejectedVacancy(Vacancy vacancy)
        {
            var rejectedViewModel = new RejectedVacancyViewModel();
            await _vacancyDisplayMapper.MapFromVacancyAsync(rejectedViewModel, vacancy);
            return new ViewVacancy
            {
                ViewModel = rejectedViewModel,
                ViewName = ViewNames.ManageRejectedVacancyView
            };
        }
    }
}
