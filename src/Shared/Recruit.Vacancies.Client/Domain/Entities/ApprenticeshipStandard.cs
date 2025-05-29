using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public class ApprenticeshipStandard
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

    public static implicit operator ApprenticeshipStandard(GetVacancyPreviewApiResponse source)
    {
        if (source == null)
        {
            return null;
        }
        return new ApprenticeshipStandard
        {
            ApprenticeshipLevel = source.ApprenticeshipLevel,
            Level = source.Level,
            ApprenticeshipType = source.ApprenticeshipType,
            Title = source.Title,
            CoreDuties = source.CoreDuties,
            EducationLevelNumber = source.EducationLevelNumber,
            OverviewOfRole = source.OverviewOfRole,
            StandardPageUrl = source.StandardPageUrl,
            Skills = source.Skills
        };
    }
}