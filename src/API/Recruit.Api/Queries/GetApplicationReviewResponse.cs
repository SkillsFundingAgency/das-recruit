using System;
using SFA.DAS.Recruit.Api.Models;

namespace SFA.DAS.Recruit.Api.Queries;

public class GetApplicationReviewResponse : ResponseBase
{
    public ApplicationReviewResponse ApplicationReview { get; set; }
}

public record ApplicationReviewResponse
{
    public Guid ApplicationReviewId { get; set; }
    public Guid CandidateId { get; set; }
    public long VacancyReference { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string AddressLine3 { get; set; }
    public string AddressLine4 { get; set; }
    public string Postcode { get; set; }
    public DateTime ApplicationDate { get; set; }
    public DateTime BirthDate { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
}