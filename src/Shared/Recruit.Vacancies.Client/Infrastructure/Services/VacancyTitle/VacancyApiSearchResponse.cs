
using System.Collections.Generic;

public class VacancyApiSearchResponse
{
    public long TotalMatched { get; set; }
    public long TotalReturned { get; set; }
    public int CurrentPage { get; set; }
    public double TotalPages { get; set; }
    public string SortBy { get; set; }
    public IEnumerable<VacancyApiApprenticeshipSearchResult> Results { get; set; }
}

public class VacancyApiApprenticeshipSearchResult
{
    public int VacancyReference { get; set; }
    public string Title { get; set; }
    public string ShortDescription { get; set; }
    public string ExpectedStartDate { get; set; }
    public string PostedDate { get; set; }
    public string ApplicationClosingDate { get; set; }
    public int NumberOfPositions { get; set; }
    public string TrainingType { get; set; }
    public string TrainingTitle { get; set; }
    public string TrainingCode { get; set; }
    public string EmployerName { get; set; }
    public string TrainingProviderName { get; set; }
    public bool IsNationwide { get; set; }
    public LatLong Location { get; set; }    

    public string ApprenticeshipLevel { get; set; }
    public string VacancyUrl { get; set; }
    public bool IsEmployerDisabilityConfident { get; set; }
    public double DistanceInMiles { get; set; }
}

public class LatLong
{
    public double Longitude { get; set; }
    public double Latitude  { get; set; }
} 

