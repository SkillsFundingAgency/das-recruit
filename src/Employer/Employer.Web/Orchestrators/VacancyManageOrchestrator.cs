using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyManage;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyManageOrchestrator
    {
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;
        private readonly IEmployerVacancyClient _client;

        public VacancyManageOrchestrator(DisplayVacancyViewModelMapper vacancyDisplayMapper, IEmployerVacancyClient client)
        {
            _vacancyDisplayMapper = vacancyDisplayMapper;
            _client = client;
        }

        public async Task<Vacancy> GetVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            Utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

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
                case VacancyStatus.Live:
                    var liveViewModel = new LiveVacancyViewModel();
                    await _vacancyDisplayMapper.MapFromVacancyAsync(liveViewModel, vacancy);
                    return liveViewModel;
                case VacancyStatus.Closed:
                    var closedViewModel = new ClosedVacancyViewModel();
                    await _vacancyDisplayMapper.MapFromVacancyAsync(closedViewModel, vacancy);
                    return closedViewModel;
                default:
                    throw new InvalidStateException(string.Format(ErrorMessages.VacancyCannotBeViewed, vacancy.Title));
            }
        }

        /// <summary>
        /// Gets vacancy for display with applications (where available)
        /// </summary>
        /// <param name="vacancy"></param>
        /// <returns></returns>
        public async Task<ManageVacancy> GetVacancyDisplayViewModelAsync(Vacancy vacancy)
        {
            switch (vacancy.Status)
            {
                case VacancyStatus.Submitted:
                case VacancyStatus.PendingReview:
                case VacancyStatus.UnderReview:
                    return await GetDisplayViewModelForSubmittedVacancy(vacancy);
                case VacancyStatus.Approved:
                    return await GetDisplayViewModelForApprovedVacancy(vacancy);
                case VacancyStatus.Live:
                    return GetDisplayViewModelForLiveVacancy(vacancy);
                case VacancyStatus.Closed:
                    return GetDisplayViewModelForClosedVacancy(vacancy);
                case VacancyStatus.Referred:
                    return await GetDisplayViewModelForReferredVacancy(vacancy);
                default:
                    throw new InvalidStateException(string.Format(ErrorMessages.VacancyCannotBeViewed, vacancy.Title));
            }
        }

        private async Task<ManageVacancy> GetDisplayViewModelForSubmittedVacancy(Vacancy vacancy)
        {
            var submittedViewModel = new SubmittedVacancyViewModel();
            await _vacancyDisplayMapper.MapFromVacancyAsync(submittedViewModel, vacancy);
            submittedViewModel.SubmittedDate = vacancy.SubmittedDate.Value.AsGdsDate();
            return new ManageVacancy
            {
                ViewModel = submittedViewModel,
                ViewName = ViewNames.ManageSubmittedVacancyView
            };
        }

        private async Task<ManageVacancy> GetDisplayViewModelForApprovedVacancy(Vacancy vacancy)
        {
            var approvedViewModel = new ApprovedVacancyViewModel();
            await _vacancyDisplayMapper.MapFromVacancyAsync(approvedViewModel, vacancy);
            approvedViewModel.ApprovedDate = vacancy.ApprovedDate.Value.AsGdsDate();
            return new ManageVacancy
            {
                ViewModel = approvedViewModel,
                ViewName = ViewNames.ManageApprovedVacancyView
            };
        }

        private ManageVacancy GetDisplayViewModelForLiveVacancy(Vacancy vacancy)
        {
            var liveViewModel = new LiveVacancyViewModel();
            PopulateViewModelWithApplications(vacancy, liveViewModel);
            return new ManageVacancy
            {
                ViewModel = liveViewModel,
                ViewName = liveViewModel.HasApplications ? ViewNames.ManageLiveVacancyWithApplicationsView : ViewNames.ManageLiveVacancyView
            };
        }

        private ManageVacancy GetDisplayViewModelForClosedVacancy(Vacancy vacancy)
        {
            var closedViewModel = new ClosedVacancyViewModel();
            PopulateViewModelWithApplications(vacancy, closedViewModel);
            closedViewModel.ClosedDate = vacancy.ClosedDate.Value.AsGdsDate();
            return new ManageVacancy
            {
                ViewModel = closedViewModel,
                ViewName = closedViewModel.HasApplications ? ViewNames.ManageClosedVacancyWithApplicationsView : ViewNames.ManageClosedVacancyView
            };
        }

        private void PopulateViewModelWithApplications(Vacancy vacancy, DisplayVacancyApplicationViewModel viewModel)
        {
            var mappedDisplayVacancyViewModelTask = _vacancyDisplayMapper.MapFromVacancyAsync(viewModel, vacancy);
            var vacancyApplicationsTask = _client.GetVacancyApplicationsAsync(vacancy.VacancyReference.Value.ToString());

            Task.WaitAll(mappedDisplayVacancyViewModelTask, vacancyApplicationsTask);

            var applications = vacancyApplicationsTask.Result?.Applications ?? new List<VacancyApplication>();

            viewModel.Applications = new VacancyApplicationsViewModel
            {
                Applications = applications,
                ShowDisability = vacancy.IsDisabilityConfident
            };
        }

        private async Task<ManageVacancy> GetDisplayViewModelForReferredVacancy(Vacancy vacancy)
        {
            var referredViewModel = new ReferredVacancyViewModel();
            await _vacancyDisplayMapper.MapFromVacancyAsync(referredViewModel, vacancy);
            return new ManageVacancy
            {
                ViewModel = referredViewModel,
                ViewName = ViewNames.ManageReferredVacancyView
            };
        }
    }
}
