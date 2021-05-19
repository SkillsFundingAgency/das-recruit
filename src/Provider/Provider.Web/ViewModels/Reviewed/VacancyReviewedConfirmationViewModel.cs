namespace Esfa.Recruit.Provider.Web.ViewModels.Reviewed
{
    public class VacancyReviewedConfirmationViewModel
    {
        public string Title { get; set; }
        public string VacancyReference { get; set; }

        public bool HasVacancyReference => !string.IsNullOrEmpty(VacancyReference);
        public string EmployerName { get; set; }
    }
}
