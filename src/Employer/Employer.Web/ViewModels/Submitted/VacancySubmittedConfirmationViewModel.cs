namespace Esfa.Recruit.Employer.Web.ViewModels.Submitted
{
    public class VacancySubmittedConfirmationViewModel
    {
        public string Title { get; set; }
        public string VacancyReference { get; set; }

        public bool HasVacancyReference => !string.IsNullOrEmpty(VacancyReference);
    }
}
