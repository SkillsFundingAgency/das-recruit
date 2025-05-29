using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public class GetVacancyPreviewApiResponse
{
    public string OverviewOfRole { get; set; }
    public List<string> Skills { get; set; }
    public List<string> CoreDuties { get; set; }
    public string StandardPageUrl { get; set; }
    public int Level { get; set; }
    public string Title { get; set; }
    public ApprenticeshipLevel ApprenticeshipLevel { get; set;}
    public int EducationLevelNumber { get; set; }
    public TrainingType ApprenticeshipType { get; set; }
}