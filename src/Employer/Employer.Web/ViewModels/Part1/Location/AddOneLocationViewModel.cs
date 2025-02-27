﻿using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Location;

public class AddOneLocationViewModel: AddOneLocationEditModel
{
    public string ApprenticeshipTitle { get; init; }
    public List<Address> AvailableLocations { get; set; } = [];
    public string BannerAddress { get; set; }
    public PartOnePageInfoViewModel PageInfo { get; init; }
}