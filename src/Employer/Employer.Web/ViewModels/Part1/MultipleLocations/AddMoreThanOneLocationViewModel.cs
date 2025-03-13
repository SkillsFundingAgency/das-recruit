using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.MultipleLocations;

public class AddMoreThanOneLocationViewModel : AddMoreThanOneLocationEditModel
{
    public string ApprenticeshipTitle { get; init; }
    public List<Address> AvailableLocations { get; set; } = [];
    public IOrderedEnumerable<IGrouping<string, KeyValuePair<string, Address>>> GroupedLocations { get; set; }
    public string BannerAddress { get; set; }
    public PartOnePageInfoViewModel PageInfo { get; init; }
    public ReviewSummaryViewModel Review { get; set; } = new ();
}