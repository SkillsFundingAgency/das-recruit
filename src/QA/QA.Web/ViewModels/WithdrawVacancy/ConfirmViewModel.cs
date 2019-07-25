using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Qa.Web.ViewModels.WithdrawVacancy
{
    public class ConfirmViewModel
    {
        public long VacancyReference { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string LegalEntityName { get; set; }
        public string TrainingProvider { get; set; }
        public OwnerType Owner { get; set; }

        public bool IsEmployerOwned => Owner == OwnerType.Employer;
        public bool IsProviderOwned => Owner == OwnerType.Provider;
    }
}
