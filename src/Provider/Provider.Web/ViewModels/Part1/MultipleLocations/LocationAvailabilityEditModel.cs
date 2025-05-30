﻿using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.MultipleLocations;

public class LocationAvailabilityEditModel : VacancyRouteModel
{
    [Required(ErrorMessage = ValidationMessages.MultipleLocationMessages.SelectionRequired)]
    public AvailableWhere? SelectedAvailability { get; init; }
}