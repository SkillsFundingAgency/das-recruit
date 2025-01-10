using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Services;
using ApplicationReviewStatus = Esfa.Recruit.Vacancies.Client.Domain.Entities.ApplicationReviewStatus;

namespace SFA.DAS.Recruit.Api.Mappers;

public static class ApplicantSummaryMapper
{
    public static ApplicantSummary MapFromVacancyApplication(VacancyApplication va)
    {
        return new ApplicantSummary
        {
            ApplicantId = va.CandidateId,
            FirstName = va.FirstName,
            LastName = va.LastName,
            DateOfBirth = va.DateOfBirth,
            ApplicationStatus = va.Status == (ApplicationReviewStatus)Services.ApplicationReviewStatus.New ? "N/A" : va.Status.ToString()
        };
    }
}