﻿using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.ProviderContactDetails;

public class ProviderContactDetailsViewModel : TaskListViewModel
{
    public string Title { get; internal set; }
    public string ProviderContactName { get; internal set; }
    public string ProviderContactEmail { get; internal set; }
    public string ProviderContactPhone { get; internal set; }
    public string ProviderName { get; internal set; }
    public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
    public IList<string> OrderedFieldNames => new List<string>
    {
        nameof(ProviderContactName),
        nameof(ProviderContactEmail),
        nameof(ProviderContactPhone)
    };
    public bool? AddContactDetails { get; set; }
    public bool IsTaskListCompleted { get; set; }

    public string PageBackLink
    {
        get
        {
            return IsTaskListCompleted
                ? RouteNames.ProviderCheckYourAnswersGet
                : RouteNames.AboutEmployer_Get;
        }
    }
}