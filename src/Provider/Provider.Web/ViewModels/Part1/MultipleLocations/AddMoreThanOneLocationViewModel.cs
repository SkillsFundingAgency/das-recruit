using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.MultipleLocations;

public class AddMoreThanOneLocationViewModel : AddMoreThanOneLocationEditModel
{
    public string ApprenticeshipTitle { get; init; }
    public List<Address> AvailableLocations { get; set; } = [];
    public string BannerAddress { get; set; }
    public PartOnePageInfoViewModel PageInfo { get; init; }
}