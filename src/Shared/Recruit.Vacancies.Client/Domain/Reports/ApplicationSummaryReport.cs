using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Reports;
public record ApplicationSummaryReport
{

    public string CandidateName { get; set; } = "";
    public string CandidateId { get; set; } = "";
    public string AddressLine1 { get; set; } = "";
    public string AddressLine2 { get; set; } = "";
    public string Town { get; set; } = "";
    public string County { get; set; } = "";
    public string Postcode { get; set; } = "";
    public string Email { get; set; } = "";
    public string Telephone { get; set; } = "";
    public string DateOfBirth { get; set; } = "";
    public long VacancyReferenceNumber { get; set; }
    public string VacancyTitle { get; set; } = "";
    public string VacancyPostcode { get; set; } = "";
    public string CourseName { get; set; } = "";
    public string CourseId { get; set; }
    public string CourseStatus { get; set; } = "";
    public int ApprenticeshipLevel { get; set; } = 0;
    public string ApprenticeshipType { get; set; } = ApprenticeshipTypes.Standard.ToString();
    public string Employer { get; set; } = "";
    public string LearningProvider { get; set; } = "";
    public DateTime? ApplicationDate { get; set; }
    public DateTime? VacancyClosingDate { get; set; }
    public string ApplicationStatus { get; set; }
    public int NumberOfDaysApplicationAtThisStatus { get; set; } = 0;
    public string InterviewAssistance { get; set; } = "";
    public bool RecruitingNationally { get; set; }
    public string Workplace1 { get; set; } = "";
    public string Workplace2 { get; set; } = "";
    public string Workplace3 { get; set; } = "";
    public string Workplace4 { get; set; } = "";
    public string Workplace5 { get; set; } = "";
    public string Workplace6 { get; set; } = "";
    public string Workplace7 { get; set; } = "";
    public string Workplace8 { get; set; } = "";
    public string Workplace9 { get; set; } = "";
    public string Workplace10 { get; set; } = "";
}