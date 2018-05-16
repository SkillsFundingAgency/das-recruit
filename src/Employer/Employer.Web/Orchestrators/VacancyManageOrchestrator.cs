using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyManageOrchestrator
    {
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;
        private readonly ITimeProvider _timeProvider;
        private readonly IEmployerVacancyClient _client;

        public VacancyManageOrchestrator(DisplayVacancyViewModelMapper vacancyDisplayMapper, ITimeProvider timeProvider, IEmployerVacancyClient client)
        {
            _vacancyDisplayMapper = vacancyDisplayMapper;
            _timeProvider = timeProvider;
            _client = client;
        }

        public async Task<Vacancy> GetVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            Utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            return vacancy;
        }

        public async Task<ManageVacancy> GetVacancyDisplayViewModelAsync(Vacancy vacancy)
        {
            switch (vacancy.Status)
            {
                case VacancyStatus.Submitted:
                case VacancyStatus.PendingReview:
                case VacancyStatus.UnderReview:
                    var submittedViewModel = new SubmittedVacancyViewModel();
                    await _vacancyDisplayMapper.MapFromVacancyAsync(submittedViewModel, vacancy);
                    submittedViewModel.SubmittedDate = vacancy.SubmittedDate.Value.AsDisplayDate();
                    return new ManageVacancy
                    {
                        ViewModel = submittedViewModel,
                        ViewName = ViewNames.ManageSubmittedVacancyView
                    };
                case VacancyStatus.Approved:
                    var approvedViewModel = new ApprovedVacancyViewModel();
                    await _vacancyDisplayMapper.MapFromVacancyAsync(approvedViewModel, vacancy);
                    approvedViewModel.ApprovedDate = vacancy.ApprovedDate.Value.AsDisplayDate();
                    return new ManageVacancy
                    {
                        ViewModel = approvedViewModel,
                        ViewName = ViewNames.ManageApprovedVacancyView
                    };
                case VacancyStatus.Live:
                    var liveViewModel = new LiveVacancyViewModel();
                    await _vacancyDisplayMapper.MapFromVacancyAsync(liveViewModel, vacancy);
                    return new ManageVacancy
                    {
                        ViewModel = liveViewModel,
                        ViewName = ViewNames.ManageLiveVacancyView
                    };
                case VacancyStatus.Closed:
                    return await GetClosedVacancyViewModel(vacancy);
                case VacancyStatus.Referred:
                    var referredViewModel = new ReferredVacancyViewModel();
                    await _vacancyDisplayMapper.MapFromVacancyAsync(referredViewModel, vacancy);
                    return new ManageVacancy
                    {
                        ViewModel = referredViewModel,
                        ViewName = ViewNames.ManageReferredVacancyView
                    };
                default:
                    throw new InvalidStateException(string.Format(ErrorMessages.VacancyCannotBeViewed, vacancy.Title));
            }
        }

        private async Task<ManageVacancy> GetClosedVacancyViewModel(Vacancy vacancy)
        {
            var closedViewModel = new ClosedVacancyViewModel();
            await _vacancyDisplayMapper.MapFromVacancyAsync(closedViewModel, vacancy);
            return new ManageVacancy
            {
                ViewModel = closedViewModel,
                ViewName = ViewNames.ManageClosedVacancyView
            };
        }
    }
}
