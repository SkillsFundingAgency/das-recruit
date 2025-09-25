using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.AddLocation;

public class EnterLocationManuallyViewModel : AddLocationJourneyViewModel
{
    public string ReturnRoute { get; init; }
    public string AddressLine1 { get; init; }
    public string AddressLine2 { get; init; }
    public string City { get; init; }
    public string Postcode { get; init; }
    
    public IList<string> OrderedFieldNames => new List<string>
    {
        nameof(AddressLine1),
        nameof(AddressLine2),
        nameof(City),
        nameof(Postcode)
    };
}