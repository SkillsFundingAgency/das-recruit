using Esfa.Recruit.Provider.Web.Models.AddLocation;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.AddLocation;

public class AddLocationJourneyViewModel : AddLocationJourneyModel
{
    public string ApprenticeshipTitle { get; init; }
    public PartOnePageInfoViewModel PageInfo { get; init; }
}