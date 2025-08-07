using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses;
public record ApplicationReview
{
    public bool HasEverBeenEmployerInterviewing { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? DateSharedWithEmployer { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public DateTime? StatusUpdatedDate { get; set; }
    public DateTime SubmittedDate { get; set; }
    public DateTime? WithdrawnDate { get; set; }
    public Guid CandidateId { get; set; }
    public Guid Id { get; set; }
    public Guid? ApplicationId { get; set; }
    public Guid? LegacyApplicationId { get; set; }
    public int Ukprn { get; set; }
    public long AccountId { get; set; }
    public long AccountLegalEntityId { get; set; }
    public long VacancyReference { get; set; }
    public string Status { get; set; }
    public string VacancyTitle { get; set; }
    public string? AdditionalQuestion1 { get; set; }
    public string? AdditionalQuestion2 { get; set; }
    public string? CandidateFeedback { get; set; }
    public string? EmployerFeedback { get; set; }
    public string? TemporaryReviewStatus { get; set; }
    public Application? Application { get; set; } = null;
}

public record Candidate
{
    public DateTime? DateOfBirth { get; set; }
    public string? Email { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? PhoneNumber { get; set; }

    public CandidateAddress? Address { get; set; } = null;

    public record CandidateAddress
    {
        public string? AddressLine1 { get; init; }
        public string? AddressLine2 { get; init; }
        public string? Town { get; init; }
        public string? County { get; init; }
        public string? Postcode { get; init; }
    }
}

public record Application
{

    public bool? ApplyUnderDisabilityConfidentScheme { get; set; }
    public Candidate? Candidate { get; set; } = null;
    public DateTime CreatedDate { get; set; }
    public DateTime? MigrationDate { get; set; }
    public DateTime? ResponseDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? WithdrawnDate { get; set; }
    public Guid CandidateId { get; set; }
    public Guid Id { get; set; }
    public List<AdditionalQuestion>? AdditionalQuestions { get; set; } = [];
    public List<Qualification> Qualifications { get; set; } = [];
    public List<TrainingCourseItem> TrainingCourses { get; set; } = [];
    public List<WorkHistoryItem> WorkHistory { get; set; } = [];  
    public Location? EmploymentLocation { get; set; }
    public string? ResponseNotes { get; set; }
    public string? Strengths { get; set; }
    public string? Support { get; set; }
    public string? WhatIsYourInterest { get; set; }
}

public record Location
{
    public List<Address>? Addresses { get; set; } = [];
}

public record Address
{
    public bool IsSelected { get; init; }
    public string? FullAddress { get; init; }
}

public record AdditionalQuestion
{
    public string? QuestionText { get; init; }
    public string? Answer { get; init; }
}

public record Qualification
{
    public string? Subject { get; set; }
    public string? Grade { get; set; }
    public string? AdditionalInformation { get; set; }
    public int? ToYear { get; set; }
    public bool? IsPredicted { get; set; }
    public short? QualificationOrder { get; set; }
    public QualificationReference QualificationReference { get; set; }
}

public record QualificationReference
{
    public string? Name { get; set; }
}

public class WorkHistoryItem
{
    public WorkHistoryType WorkHistoryType { get; set; }
    public string? Employer { get; set; }
    public string? JobTitle { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
}

public record TrainingCourseItem
{
    public string CourseName { get; set; }
    public int YearAchieved { get; set; }
}

public enum WorkHistoryType : byte
{
    Job = 0,
    WorkExperience = 1
}