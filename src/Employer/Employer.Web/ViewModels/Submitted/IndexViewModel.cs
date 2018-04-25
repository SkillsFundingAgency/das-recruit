namespace Esfa.Recruit.Employer.Web.ViewModels.Submitted
{
    public class IndexViewModel
    {
        public string Title { get; set; }
        public string VacancyReference { get; set; }

        public bool HasVacancyReference => !string.IsNullOrEmpty(VacancyReference);
    }
}
