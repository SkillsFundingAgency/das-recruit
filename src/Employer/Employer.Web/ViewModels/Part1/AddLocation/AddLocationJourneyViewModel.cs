using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.AddLocation;

public class AddLocationJourneyViewModel : AddLocationJourneyModel
{
    public string ApprenticeshipTitle { get; init; }
    public PartOnePageInfoViewModel PageInfo { get; init; }
}