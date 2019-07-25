namespace Esfa.Recruit.Qa.Web.ViewModels.WithdrawVacancy
{
    public class AcknowledgeViewModel
    {
        public long VacancyReference { get; set; }
        public string OwnerName { get; set; }
        public bool Acknowledged { get; set; }
    }
}
