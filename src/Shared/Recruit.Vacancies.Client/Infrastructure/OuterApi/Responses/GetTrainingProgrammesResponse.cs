using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public class GetTrainingProgrammesResponse
{
    public IEnumerable<GetTrainingProgrammesResponseItem> TrainingProgrammes { get; set; }
}

public class GetTrainingProgrammesResponseItem
{
    public string Id { get; set; }
    public TrainingType ApprenticeshipType { get; set;}
    public string Title { get; set;}
    public DateTime? EffectiveFrom { get; set;}
    public DateTime? EffectiveTo { get; set;}
    public DateTime? LastDateStarts { get; set; }      
    public ApprenticeshipLevel ApprenticeshipLevel { get; set;}
    public int Duration { get; set;}
    public bool IsActive { get; set; }
    public int? EducationLevelNumber { get; set; }
}