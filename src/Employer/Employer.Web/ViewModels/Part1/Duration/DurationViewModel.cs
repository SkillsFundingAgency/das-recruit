﻿using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels.Validations;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.DurationValidationMessages;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Duration
{
    public class DurationViewModel : VacancyRouteModel
    {
        [TypeOfInteger(ErrorMessage = ErrMsg.TypeOfInteger.Duration)]
        public string Duration { get; set; }

        public DurationUnit DurationUnit { get; set; }

        public string WorkingWeekDescription { get; set; }

        [TypeOfDecimal(2, ErrorMessage = ErrMsg.TypeOfDecimal.WeeklyHours)]
        public string WeeklyHours { get; set; }

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public string TrainingTitle { get; set; }
        public int TrainingDurationMonths { get; set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(Duration),
            nameof(WorkingWeekDescription),
            nameof(WeeklyHours),
        };

        public PartOnePageInfoViewModel PageInfo { get; set; }

        public bool ShowTraining => string.IsNullOrWhiteSpace(TrainingTitle) == false && TrainingDurationMonths > 0;
        public string VacancyTitle { get; init; }
    }
}
