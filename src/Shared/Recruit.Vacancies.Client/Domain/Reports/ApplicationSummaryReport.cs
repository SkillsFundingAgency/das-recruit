using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Newtonsoft.Json;

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

public class ApplicationSummaryCsvReport
{
    [JsonProperty("Candidate_Name")]
    public string CandidateName { get; set; } = "";
    [JsonProperty("Applicant_Id")]
    public string CandidateId { get; set; }
    [JsonProperty("Address_Line1")]
    public string AddressLine1 { get; set; } = "";
    [JsonProperty("Address_Line2")]
    public string AddressLine2 { get; set; } = "";
    [JsonProperty("Address_Line3")]
    public string Town { get; set; } = "";
    [JsonProperty("Address_Line4")]
    public string County { get; set; } = "";
    [JsonProperty("Postcode")]
    public string Postcode { get; set; } = "";
    [JsonProperty("Telephone")]
    public string Telephone { get; set; } = "";
    [JsonProperty("Email")]
    public string Email { get; set; } = "";
    [JsonProperty("School")]
    public string School { get; set; } = "";
    [JsonProperty("Date_of_Birth")]
    public string DateOfBirth { get; set; } = "";
    [JsonProperty("Vacancy_Reference_Number")]
    public string VacancyReferenceNumber { get; set; }
    [JsonProperty("Vacancy_Title")]
    public string VacancyTitle { get; set; } = "";
    [JsonProperty("Framework")]
    public string Framework { get; set; } = "";
    [JsonProperty("Framework_Status")]
    public string FrameworkStatus { get; set; } = "";
    [JsonProperty("Standard")]
    public string CourseName { get; set; } = "";
    [JsonProperty("Standard_Status")]
    public string CourseStatus { get; set; } = "";
    [JsonProperty("RouteId")]
    public string RouteId { get; set; } = "RouteId";
    [JsonProperty("Employer")]
    public string Employer { get; set; } = "";
    [JsonProperty("Vacancy_Postcode")]
    public string VacancyPostcode { get; set; } = "";
    [JsonProperty("Learning_Provider")]
    public string LearningProvider { get; set; } = "";
    [JsonProperty("Application_Date")]
    public string ApplicationDate { get; set; }
    [JsonProperty("Vacancy_Closing_Date")]
    public string VacancyClosingDate { get; set; }
    [JsonProperty("Application_Status")]
    public string ApplicationStatus { get; set; }
    [JsonProperty("Number_Of_Days_App_At_This_Status")]
    public int NumberOfDaysApplicationAtThisStatus { get; set; } = 0;

    public static implicit operator ApplicationSummaryCsvReport(ApplicationSummaryReport report)
    {
        return new ApplicationSummaryCsvReport
        {
            CandidateName = report.CandidateName,
            CandidateId = report.CandidateId.Substring(0,7),
            AddressLine1 = report.AddressLine1,
            AddressLine2 = report.AddressLine2,
            Town = report.Town,
            County = report.County,
            Postcode = report.Postcode,
            Email = report.Email,
            Telephone = report.Telephone,
            DateOfBirth = report.DateOfBirth,
            VacancyReferenceNumber = $"VAC{report.VacancyReferenceNumber}",
            VacancyTitle = report.VacancyTitle,
            CourseName = $"{report.CourseId} {report.CourseName}",
            CourseStatus = report.CourseStatus.ToLower() == "approved for delivery" ? "Active" : "Inactive",
            Employer = report.Employer,
            VacancyPostcode = report.VacancyPostcode,
            LearningProvider = report.LearningProvider,
            ApplicationDate = report.ApplicationDate!.Value.ToShortDateString(),
            VacancyClosingDate = report.VacancyClosingDate!.Value.ToShortDateString(),
            ApplicationStatus = report.ApplicationStatus,
            NumberOfDaysApplicationAtThisStatus = report.NumberOfDaysApplicationAtThisStatus
        };
    }
}