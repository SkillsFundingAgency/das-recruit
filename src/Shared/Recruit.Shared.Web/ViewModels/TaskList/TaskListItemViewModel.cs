using System.Collections.Generic;

namespace Esfa.Recruit.Shared.Web.ViewModels.TaskList;

public record TaskListItemViewModel(
    bool Complete,
    string Text,
    string Route,
    Dictionary<string, string> RouteValues,
    bool Disabled = false);
    
public record DisabledTaskListItemViewModel(string Text) : TaskListItemViewModel(false, Text, null, null, true);