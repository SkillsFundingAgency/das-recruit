using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.Reports.ReportConfirmation
{
    public class ConfirmationViewModel
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public long Ukprn { get; set; }
        public VacancyType VacancyType { get; set; }
    }
}
