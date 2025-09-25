using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Provider.Web.Models.AddLocation;

public class SelectLocationEditModel : AddLocationJourneyModel
{
    [Required(ErrorMessage = "Select your address or select ‘Enter address manually’")]
    public string SelectedLocation { get; init; }
}