﻿using System;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview
{
    public class TrainingCoursesViewModel
    {
        public string Provider { get; set; }
        public string Title { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string FromDateAsText => FromDate.ToMonthYearString();
        public string ToDateAsText => ToDate.ToMonthYearString();
    }
}