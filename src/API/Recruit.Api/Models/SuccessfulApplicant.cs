using System;

namespace SFA.DAS.Recruit.Api.Models;

public record SuccessfulApplicant
{
    public long? VacancyReference { get; set; }
    public Guid ApplicationReviewId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Guid CandidateId { get; set; }
}