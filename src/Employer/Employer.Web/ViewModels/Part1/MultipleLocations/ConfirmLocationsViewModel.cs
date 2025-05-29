using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.MultipleLocations;

public class ConfirmLocationsViewModel : VacancyRouteModel
{
    public string ApprenticeshipTitle { get; init; }
    public List<Address> Locations { get; init; }
    public PartOnePageInfoViewModel PageInfo { get; init; }
}