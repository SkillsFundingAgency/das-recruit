using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Shared.Web.Mappers
{
    public class VacancySummaryMapper
    {
        public static VacancySummaryViewModel ConvertToVacancySummaryViewModel(VacancySummary vacancySummary)
        {
            var summaryViewModel = new VacancySummaryViewModel
            {
                Id = vacancySummary.Id,
                Title = vacancySummary.Title,
                VacancyReference = vacancySummary.VacancyReference,
                EmployerName = string.IsNullOrWhiteSpace(vacancySummary.LegalEntityName) ? "Not selected" : vacancySummary.LegalEntityName,
                CreatedDate = vacancySummary.CreatedDate,
                Status = vacancySummary.Status,
                NoOfNewApplications = vacancySummary.NoOfNewApplications,
                NoOfSuccessfulApplications = vacancySummary.NoOfSuccessfulApplications,  
                NoOfUnsuccessfulApplications = vacancySummary.NoOfUnsuccessfulApplications,
                NoOfSharedApplications = vacancySummary.NoOfSharedApplications,
                NoOfAllSharedApplications = vacancySummary.NoOfAllSharedApplications,
                ClosingDate = vacancySummary.ClosedDate ?? vacancySummary.ClosingDate,
                ApplicationMethod = vacancySummary.ApplicationMethod,
                ProgrammeId = vacancySummary.ProgrammeId,
                TrainingLevel = vacancySummary.TrainingLevel,
                TrainingTitle = vacancySummary.TrainingTitle,
                TrainingType = vacancySummary.TrainingType,
                IsTransferred = vacancySummary.TransferInfoTransferredDate.HasValue,
                IsTaskListCompleted = vacancySummary.IsTaskListCompleted
            };

            return summaryViewModel;
        }
    }
}
