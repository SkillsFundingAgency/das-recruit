using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels;

public record struct BackLinkViewModel(Dictionary<string, string> RouteDictionary, string PreviousStepRoute, bool IsTaskListCompleted, bool IsTaskList);