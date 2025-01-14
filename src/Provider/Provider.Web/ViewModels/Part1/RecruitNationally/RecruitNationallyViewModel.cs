using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.RecruitNationally;

public class RecruitNationallyViewModel: VacancyRouteModel
{
    public string AdditionalInformation { get; init; }
    public string ApprenticeshipTitle { get; init; }
    public PartOnePageInfoViewModel PageInfo { get; init; }
}