using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Shared.Web.Helpers;

namespace Esfa.Recruit.Employer.Web.Orchestrators
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
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            _utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            return vacancy;
        }

        /// <summary>
        /// Gets vacancy for display without applications.
        /// </summary>
        /// <param name="vacancy"></param>
        /// <returns></returns>
        public async Task<DisplayVacancyViewModel> GetFullVacancyDisplayViewModelAsync(Vacancy vacancy)
        {
            var programme = await _client.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);
            switch (vacancy.Status)
            {
                case VacancyStatus.Approved:
                    var approvedViewModel = new ApprovedVacancyViewModel { EducationLevelName = EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme.EducationLevelNumber, programme.ApprenticeshipLevel) };
                    await _vacancyDisplayMapper.MapFromVacancyAsync(approvedViewModel, vacancy);
                    approvedViewModel.ApprovedDate = vacancy.ApprovedDate.Value.AsGdsDate();
                    return approvedViewModel;
                case VacancyStatus.Live:
                    var liveViewModel = new LiveVacancyViewModel { EducationLevelName = EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme.EducationLevelNumber, programme.ApprenticeshipLevel) };
                    await _vacancyDisplayMapper.MapFromVacancyAsync(liveViewModel, vacancy);
                    return liveViewModel;
                 case VacancyStatus.Closed:
                    var closedViewModel = new ClosedVacancyViewModel { EducationLevelName = EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme.EducationLevelNumber, programme.ApprenticeshipLevel) };
                    await _vacancyDisplayMapper.MapFromVacancyAsync(closedViewModel, vacancy);
                    return closedViewModel;
                case VacancyStatus.Submitted:
                    var submittedViewModel = new SubmittedVacancyViewModel { EducationLevelName = EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme.EducationLevelNumber, programme.ApprenticeshipLevel) };
                    await _vacancyDisplayMapper.MapFromVacancyAsync(submittedViewModel, vacancy);
                    submittedViewModel.SubmittedDate = vacancy.SubmittedDate.Value.AsGdsDate();
                    return submittedViewModel;
                case VacancyStatus.Review:
                    var reviewViewModel = new ReviewVacancyViewModel { EducationLevelName = EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme.EducationLevelNumber, programme.ApprenticeshipLevel) };
                    await _vacancyDisplayMapper.MapFromVacancyAsync(reviewViewModel, vacancy);
                    return reviewViewModel;
                default:
                    throw new InvalidStateException(string.Format(ErrorMessages.VacancyCannotBeViewed, vacancy.Title));
            }
        }

    }
}
