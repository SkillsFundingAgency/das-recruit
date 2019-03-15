using System.Linq;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyView;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyManage
{
    public class ManageVacancyViewModel
    {
        public string Title { get; internal set; }
        public VacancyStatus Status { get; internal set; }
        public string VacancyReference { get; internal set; }
        public string ClosingDate { get; internal set; }
        public string PossibleStartDate { get; internal set; }
        public bool IsDisabilityConfident { get; internal set; }
        public bool IsApplyThroughFaaVacancy { get; internal set; }
        public VacancyApplicationsViewModel Applications { get; internal set; }
        public bool HasApplications => Applications.Applications.Any();
        public bool HasNoApplications => Applications.Applications == null || Applications.Applications?.Any() == false;
        public bool CanShowEditVacancyLink { get; internal set; }
        public bool CanShowCloseVacancyLink { get; internal set; }
        public bool CanShowCloneVacancyLink { get; internal set; }
        public string VacancyClosedInfoMessage { get; internal set; }
        public bool HasVacancyClosedInfoMessage => !string.IsNullOrEmpty(VacancyClosedInfoMessage);
        public string ApplicationReviewStatusHeaderInfoMessage { get; internal set; }
        public bool CanShowApplicationReviewStatusHeader => !string.IsNullOrEmpty(ApplicationReviewStatusHeaderInfoMessage);
    }
}