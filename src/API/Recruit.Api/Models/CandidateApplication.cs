using System;
using System.Collections.Generic;

namespace SFA.DAS.Recruit.Api.Models;

public class CandidateApplication
{
    public Guid ApplicationId { get; set; }
    public Guid CandidateId { get; set; }
    public string VacancyReference { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string AddressLine3 { get; set; }
    public string AddressLine4 { get; set; }
    public string Postcode { get; set; }
    public DateTime ApplicationDate { get; set; }
    public string DisabilityConfidenceStatus { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string HobbiesAndInterests { get; set; }
    public string Improvements { get; set; }
    
    public string Phone { get; set; }
    
    public List<ApplicationQualification> Qualifications { get; set; }
    public string Strengths { get; set; }
    public string Support { get; set; }
    public List<TrainingCourse> TrainingCourses { get; set; }
    public List<WorkHistory> WorkExperiences { get; set; }
    public List<WorkHistory> Jobs { get; set; }
    public AdditionalQuestion? AdditionalQuestion1 { get; set; }
    public AdditionalQuestion? AdditionalQuestion2 { get; set; }
    public string WhatIsYourInterest { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class ApplicationQualification
{
    public string QualificationType { get; set; }
    public string Subject { get; set; }
    public string Grade { get; set; }
    public bool? IsPredicted { get; set; }
    public string AdditionalInformation { get; set; }
    public short? QualificationOrder { get; set; }
}

public class TrainingCourse
{
    public string Title { get; set; }
    public DateTime ToDate { get; set; }
}
public class WorkHistory
{
    public string Employer { get; set; }
    public string JobTitle { get; set; }
    public string Description { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class AdditionalQuestion
{
    public string QuestionText { get; set; }
    public string AnswerText { get; set; }
}