using System;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Shared.Web.Domain;

public abstract class TaskListStateViewBase
{
    public Dictionary<TaskListItemFlags, bool> CompleteStates { get; protected set; }
    
    protected bool AllFlagsCompleted(TaskListItemFlags sectionFlags)
    {
        return Enum.GetValues<TaskListItemFlags>()
            .Where(flag => flag is not TaskListItemFlags.None and not TaskListItemFlags.All && sectionFlags.HasFlag(flag))
            .Aggregate(true, (current, flag) => current & CompleteStates[flag]);
    }
    
    protected bool AnyFlagsCompleted(TaskListItemFlags flags)
    {
        return Enum.GetValues<TaskListItemFlags>()
            .Where(flag => flag is not TaskListItemFlags.None and not TaskListItemFlags.All && flags.HasFlag(flag))
            .Any(flag => CompleteStates[flag]);
    }
}