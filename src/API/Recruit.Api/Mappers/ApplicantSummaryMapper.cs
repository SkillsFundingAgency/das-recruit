using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Services;

namespace SFA.DAS.Recruit.Api.Mappers
{
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
                ApplicationStatus = va.Status == ApplicationReviewStatus.New ? "N/A" : va.Status.ToString()
            };
        }
    }
}