using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.Models.AddLocation;

public class SelectLocationEditModel : SelectLocationModel
{
    [Required(ErrorMessage = "Select your address or select ‘Enter address manually’")]
    public string SelectedLocation { get; init; }
}