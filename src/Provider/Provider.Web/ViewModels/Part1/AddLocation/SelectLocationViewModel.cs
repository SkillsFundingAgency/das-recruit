using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.AddLocation;

public class SelectLocationViewModel : AddLocationJourneyViewModel
{
    public IEnumerable<string> Locations { get; init; }
    public string Postcode { get; init; }
    public string SelectedLocation { get; init; }
}