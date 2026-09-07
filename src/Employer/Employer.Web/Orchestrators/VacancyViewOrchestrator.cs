using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyView;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyViewOrchestrator(
        DisplayVacancyViewModelMapper vacancyDisplayMapper,
        IRecruitVacancyClient client,
        IUtility utility)
    {
        public async Task<Vacancy> GetVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await client.GetVacancyAsync(vrm.VacancyId);

            utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            return vacancy;
        }

        /// <summary>
        /// Gets vacancy for display without applications.
        /// </summary>
        /// <param name="vacancy"></param>
        /// <returns></returns>
        public async Task<DisplayVacancyViewModel> GetFullVacancyDisplayViewModelAsync(Vacancy vacancy)
        {
            var programme = await client.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);
            switch (vacancy.Status)
            {
                case VacancyStatus.Approved:
                    var approvedViewModel = new ApprovedVacancyViewModel { EducationLevelName = EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme?.EducationLevelNumber, programme?.ApprenticeshipLevel) };
                    await vacancyDisplayMapper.MapFromVacancyAsync(approvedViewModel, vacancy);
                    approvedViewModel.ApprovedDate = vacancy.ApprovedDate.Value.AsGdsDate();
                    return approvedViewModel;
                case VacancyStatus.Live:
                    var liveViewModel = new LiveVacancyViewModel { EducationLevelName = EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme?.EducationLevelNumber, programme?.ApprenticeshipLevel) };
                    await vacancyDisplayMapper.MapFromVacancyAsync(liveViewModel, vacancy);
                    return liveViewModel;
                case VacancyStatus.Closed:
                    var closedViewModel = new ClosedVacancyViewModel { EducationLevelName = EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme?.EducationLevelNumber, programme?.ApprenticeshipLevel) };
                    await vacancyDisplayMapper.MapFromVacancyAsync(closedViewModel, vacancy);
                    return closedViewModel;
                case VacancyStatus.Archived:
                    var archivedViewModel = new ArchivedVacancyViewModel { EducationLevelName = EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme?.EducationLevelNumber, programme?.ApprenticeshipLevel) };
                    await vacancyDisplayMapper.MapFromVacancyAsync(archivedViewModel, vacancy);
                    return archivedViewModel;
                case VacancyStatus.Submitted:
                    var submittedViewModel = new SubmittedVacancyViewModel { EducationLevelName = EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme?.EducationLevelNumber, programme?.ApprenticeshipLevel) };
                    await vacancyDisplayMapper.MapFromVacancyAsync(submittedViewModel, vacancy);
                    submittedViewModel.SubmittedDate = vacancy.SubmittedDate.Value.AsGdsDate();
                    return submittedViewModel;
                case VacancyStatus.Review:
                    var reviewViewModel = new ReviewVacancyViewModel { EducationLevelName = EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme?.EducationLevelNumber, programme?.ApprenticeshipLevel) };
                    await vacancyDisplayMapper.MapFromVacancyAsync(reviewViewModel, vacancy);
                    return reviewViewModel;
                default:
                    throw new InvalidStateException(string.Format(ErrorMessages.VacancyCannotBeViewed, vacancy.Title));
            }
        }

    }
}
