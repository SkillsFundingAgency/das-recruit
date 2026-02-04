using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Vacancies;

namespace Esfa.Recruit.Shared.Web.ViewModels;

public class VacancyListItemViewModel
{
    public ApplicationMethod? ApplicationMethod { get; set; }
    public string Applications { get; set; }
    public ApprenticeshipTypes? ApprenticeshipType { get; set; }
    public DateTime? ClosingDate { get; set; }
    public string EmployerName { get; set; }
    public Guid Id { get; set; }
    public bool IsTaskListCompleted { get; set; }
    public int NoOfAllSharedApplications { get; set; }
    public int NoOfEmployerReviewedApplications { get; set; }
    public int NoOfNewApplications { get; set; }
    public int NoOfSharedApplications { get; set; }
    public int NoOfSuccessfulApplications { get; set; }
    public int NoOfUnsuccessfulApplications { get; set; }
    public Dictionary<string, string> RouteDictionary { get; set; } = [];
    public string Title { get; set; }
    public SourceOrigin? SourceOrigin { get; set; }
    public VacancyStatus Status { get; set; }
    public long? VacancyReference { get; set; }

    // calculated fields
    public bool CanShowVacancyApplicationsCount => Status is VacancyStatus.Live or VacancyStatus.Closed &&
                                                   ApplicationMethod is Vacancies.Client.Domain.Entities.ApplicationMethod.ThroughFindAnApprenticeship or Vacancies.Client.Domain.Entities.ApplicationMethod.ThroughFindATraineeship;

    public bool HasApplications => NoOfApplications > 0;
    public bool HasEmployerReviewedApplications => NoOfEmployerReviewedApplications > 0;
    public bool HasNewApplications => NoOfNewApplications > 0;
    public bool HasNewSharedApplications => NoOfSharedApplications > 0;
    public bool HasNoApplications => !HasApplications;
    public bool HasNoSharedApplications => !HasSharedApplications;
    public bool HasSharedApplications => NoOfAllSharedApplications > 0;
    public bool HasVacancyReference => VacancyReference is not null;
    public bool IsSubmittable => Status is VacancyStatus.Draft or VacancyStatus.Referred or VacancyStatus.Rejected;
    public int NoOfApplications => NoOfNewApplications + NoOfSuccessfulApplications + NoOfUnsuccessfulApplications;

    public static VacancyListItemViewModel From(VacancyListItem item, int ukprn)
    {
        return new VacancyListItemViewModel
        {
            ApplicationMethod = item.ApplicationMethod,
            Applications = $"{item.Stats?.Applications ?? '-'}",
            ApprenticeshipType = item.ApprenticeshipType,
            ClosingDate = item.ClosingDate,
            EmployerName = item.LegalEntityName,
            Id = item.Id,
            IsTaskListCompleted = item.OwnerType is OwnerType.Provider or OwnerType.Employer && item.HasSubmittedAdditionalQuestions is true,
            NoOfAllSharedApplications = item.Stats?.AllSharedApplications ?? 0,
            NoOfEmployerReviewedApplications = item.Stats?.EmployerReviewedApplications ?? 0,
            NoOfNewApplications = item.Stats?.NewApplications ?? 0,
            NoOfSharedApplications = item.Stats?.SharedApplications ?? 0,
            NoOfSuccessfulApplications = item.Stats?.SuccessfulApplications ?? 0,
            NoOfUnsuccessfulApplications = item.Stats?.UnsuccessfulApplications ?? 0,
            RouteDictionary = new Dictionary<string, string>
            {
                ["ukprn"] = $"{ukprn}",
                ["vacancyId"] = $"{item.Id}",
            },
            SourceOrigin = item.SourceOrigin,
            Status = item.Status,
            Title = item.Title,
            VacancyReference = item.VacancyReference,
        };
    }
}