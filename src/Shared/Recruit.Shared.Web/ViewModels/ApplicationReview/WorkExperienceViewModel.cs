﻿using System;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview
{
    public class WorkExperienceViewModel
    {
        public string Employer { get; set; }
        public string JobTitle { get; set; }
        public string Description { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string FromDateAsText => FromDate.ToMonthYearString();
        public string ToDateAsText => ToDate.ToMonthYearString();
    }
}