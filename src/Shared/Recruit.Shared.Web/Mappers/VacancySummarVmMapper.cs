using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Shared.Web.Mappers
{
    public class VacancySummarVmMapper
    {
        public static VacancySummaryViewModel ConvertToVacancySummaryViewModel(VacancySummary vacancySummary)
        {
            if (vacancySummary != null)
            {
                var summaryViewModel = new VacancySummaryViewModel {
                    Id = vacancySummary.Id,
                    Title = vacancySummary.Title,
                    VacancyReference = vacancySummary.VacancyReference,
                    EmployerName = vacancySummary.EmployerName,
                    CreatedDate = vacancySummary.CreatedDate,
                    Status = vacancySummary.Status,
                    IsDeleted = vacancySummary.IsDeleted,
                    AllApplicationsCount = vacancySummary.AllApplicationsCount,
                    NewApplicationsCount = vacancySummary.NewApplicationsCount,
                    ClosingDate = vacancySummary.ClosingDate,
                    ApplicationMethod = vacancySummary.ApplicationMethod,
                    ProgrammeId = vacancySummary.ProgrammeId,
                    TrainingLevel = vacancySummary.TrainingLevel,
                    TrainingTitle = vacancySummary.TrainingTitle,
                    TrainingType = vacancySummary.TrainingType
                };
                return summaryViewModel;
            }
            return new VacancySummaryViewModel();
        }
    }
}
