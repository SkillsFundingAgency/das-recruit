﻿using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.AboutEmployer;

public class AboutEmployerViewModel : TaskListViewModel
{
    public string Title { get; internal set; }
    public string EmployerDescription { get; internal set; }
    public string EmployerTitle { get; internal set; }
    public string EmployerWebsiteUrl { get; internal set; }
    public bool IsAnonymous { get; internal set; }
    public ReviewSummaryViewModel Review { get; set; } = new();

    public IList<string> OrderedFieldNames => new List<string>
    {
        nameof(AboutEmployerEditModel.EmployerDescription),
        nameof(AboutEmployerEditModel.EmployerWebsiteUrl)
    };

    public bool IsTaskListCompleted { get ; set ; }
    public bool IsDisabilityConfident { get; set; }
}