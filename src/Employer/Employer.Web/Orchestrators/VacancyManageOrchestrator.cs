using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyManageOrchestrator
    {
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;

        public VacancyManageOrchestrator(DisplayVacancyViewModelMapper vacancyDisplayMapper)
        {
            _vacancyDisplayMapper = vacancyDisplayMapper;
        }

        public async Task<ManageVacancy> GetVacancyDisplayViewModelAsync(Vacancy vacancy)
        {
            switch (vacancy.Status)
            {
                case VacancyStatus.Submitted:
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
                    var closedViewModel = new ClosedVacancyViewModel();
                    await _vacancyDisplayMapper.MapFromVacancyAsync(closedViewModel, vacancy);
                    return new ManageVacancy
                    {
                        ViewModel = closedViewModel,
                        ViewName = ViewNames.ManageClosedVacancyView
                    };
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
    }
}
