﻿using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.AboutEmployer;

public class AboutEmployerEditModel : TaskListViewModel
{
    public string EmployerDescription { get; set; }
    public string EmployerWebsiteUrl { get; set; }
    public bool IsDisabilityConfident { get; set; }
}