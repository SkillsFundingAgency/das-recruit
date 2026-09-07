using System;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Domain.Reports;

public record ApplicationSummaryCsvReportV1
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

    public static implicit operator ApplicationSummaryCsvReportV1(ApplicationSummaryReport report) =>
        new()
        {
            CandidateName = report.CandidateName,
            CandidateId = report.CandidateId[..7],
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
            CourseStatus = report.CourseStatus.Equals("approved for delivery", StringComparison.CurrentCultureIgnoreCase) ? "Active" : "Inactive",
            Employer = report.Employer,
            VacancyPostcode = report.VacancyPostcode,
            LearningProvider = report.LearningProvider,
            ApplicationDate = report.ApplicationDate!.Value.ToShortDateString(),
            VacancyClosingDate = report.VacancyClosingDate!.Value.ToShortDateString(),
            ApplicationStatus = report.ApplicationStatus,
            NumberOfDaysApplicationAtThisStatus = report.NumberOfDaysApplicationAtThisStatus
        };
}
