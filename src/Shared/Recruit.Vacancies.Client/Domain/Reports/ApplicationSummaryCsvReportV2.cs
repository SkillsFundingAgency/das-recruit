using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Domain.Reports;

public record ApplicationSummaryCsvReportV2
{
    [JsonProperty("Candidate_Name")]
    public string CandidateName { get; set; } = "";
    [JsonProperty("Applicant_ID")]
    public string CandidateId { get; set; }
    [JsonProperty("Address_Line1")]
    public string AddressLine1 { get; set; } = "";
    [JsonProperty("Address_Line2")]
    public string AddressLine2 { get; set; } = "";
    [JsonProperty("Address_Line3")]
    public string Town { get; set; } = "";
    [JsonProperty("Postcode")]
    public string Postcode { get; set; } = "";
    [JsonProperty("Telephone")]
    public string Telephone { get; set; } = "";
    [JsonProperty("Email")]
    public string Email { get; set; } = "";
    [JsonProperty("Date_of_Birth")]
    public string DateOfBirth { get; set; } = "";
    [JsonProperty("Vacancy_Reference_Number")]
    public string VacancyReferenceNumber { get; set; }
    [JsonProperty("Vacancy_Title")]
    public string VacancyTitle { get; set; } = "";
    [JsonProperty("Course_Name")]
    public string CourseName { get; set; } = "";
    [JsonProperty("Course_Status")]
    public string CourseStatus { get; set; } = "";
    [JsonProperty("Apprenticeship_Level")]
    public int ApprenticeshipLevel { get; set; } = 0;
    [JsonProperty("Apprenticeship_Type")]
    public string ApprenticeshipType { get; set; } = ApprenticeshipTypes.Standard.ToString();
    [JsonProperty("Employer")]
    public string Employer { get; set; } = "";
    [JsonProperty("Application_date")]
    public string ApplicationDate { get; set; }
    [JsonProperty("Vacancy_Closing_Date")]
    public string VacancyClosingDate { get; set; }
    [JsonProperty("Application_Status")]
    public string ApplicationStatus { get; set; }
    [JsonProperty("Number_Of_Days_App_At_This_Status")]
    public int NumberOfDaysApplicationAtThisStatus { get; set; } = 0;
    [JsonProperty("Interview_Assistance")]
    public string InterviewAssistance { get; set; } = "";
    [JsonProperty("Recruiting_Nationally")]
    public bool RecruitingNationally { get; set; }
    [JsonProperty("Workplace_1")]
    public string Workplace1 { get; set; } = "";
    [JsonProperty("Workplace_2")]
    public string Workplace2 { get; set; } = "";
    [JsonProperty("Workplace_3")]
    public string Workplace3 { get; set; } = "";
    [JsonProperty("Workplace_4")]
    public string Workplace4 { get; set; } = "";
    [JsonProperty("Workplace_5")]
    public string Workplace5 { get; set; } = "";
    [JsonProperty("Workplace_6")]
    public string Workplace6 { get; set; } = "";
    [JsonProperty("Workplace_7")]
    public string Workplace7 { get; set; } = "";
    [JsonProperty("Workplace_8")]
    public string Workplace8 { get; set; } = "";
    [JsonProperty("Workplace_9")]
    public string Workplace9 { get; set; } = "";
    [JsonProperty("Workplace_10")]
    public string Workplace10 { get; set; } = "";

    public static implicit operator ApplicationSummaryCsvReportV2(ApplicationSummaryReport report) =>
        new()
        {
            CandidateName = report.CandidateName,
            CandidateId = report.CandidateId[..7],
            AddressLine1 = report.AddressLine1,
            AddressLine2 = report.AddressLine2,
            Town = report.Town,
            Postcode = report.Postcode,
            Email = report.Email,
            Telephone = report.Telephone,
            DateOfBirth = report.DateOfBirth,
            VacancyReferenceNumber = $"VAC{report.VacancyReferenceNumber}",
            VacancyTitle = report.VacancyTitle,
            CourseName = $"{report.CourseId} {report.CourseName}",
            CourseStatus = report.CourseStatus.Equals("approved for delivery", StringComparison.CurrentCultureIgnoreCase) ? "Active" : "Inactive",
            ApprenticeshipLevel = report.ApprenticeshipLevel,
            ApprenticeshipType = report.ApprenticeshipType,
            Employer = report.Employer,
            ApplicationDate = report.ApplicationDate!.Value.ToShortDateString(),
            VacancyClosingDate = report.VacancyClosingDate!.Value.ToShortDateString(),
            ApplicationStatus = report.ApplicationStatus,
            NumberOfDaysApplicationAtThisStatus = report.NumberOfDaysApplicationAtThisStatus,
            InterviewAssistance = report.InterviewAssistance,
            RecruitingNationally = report.RecruitingNationally,
            Workplace1 = report.Workplace1,
            Workplace2 = report.Workplace2,
            Workplace3 = report.Workplace3,
            Workplace4 = report.Workplace4,
            Workplace5 = report.Workplace5,
            Workplace6 = report.Workplace6,
            Workplace7 = report.Workplace7,
            Workplace8 = report.Workplace8,
            Workplace9 = report.Workplace9,
            Workplace10 = report.Workplace10
        };
}