using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyApplication
{
    public class ApplicationSubmitCommand : CommandBase
    {
        public Application Application { get; set; }
    }
}