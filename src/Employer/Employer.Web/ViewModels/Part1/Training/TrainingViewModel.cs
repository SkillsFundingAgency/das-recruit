using System;
using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Configuration.Routing;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Training
{
    public class TrainingViewModel : TrainingEditModel
    {
        public IEnumerable<ApprenticeshipProgrammeViewModel> Programmes { get; set; }

        public VacancyRouteParameters CancelButtonRouteParameters { get; set; }
        public bool ShowStep => CancelButtonRouteParameters.RouteName != RouteNames.Vacancy_Preview_Get;
        public string SubmitButtonText => CancelButtonRouteParameters.RouteName == RouteNames.Vacancy_Preview_Get ? "Save and Preview" : "Save and Continue";

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ClosingDate),
            nameof(StartDate),
            nameof(SelectedProgrammeId)
        };
    }

    public class ApprenticeshipProgrammeViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
