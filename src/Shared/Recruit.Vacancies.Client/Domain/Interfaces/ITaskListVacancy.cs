using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Interfaces;

public interface ITaskListVacancy
{
    ApplicationMethod? ApplicationMethod { get; set; }
    bool? HasChosenProviderContactDetails { get; set; }
    bool HasSubmittedAdditionalQuestions { get; set; }
    OwnerType OwnerType { get; set; }
}