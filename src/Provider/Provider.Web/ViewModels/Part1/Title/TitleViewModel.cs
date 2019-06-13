﻿using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Title
{
    public class TitleViewModel
    {
        public Guid? VacancyId { get; set; }
        public string EmployerAccountId { get; set; }
        public long Ukprn { get; set; }
        public string Title { get; set; }
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(Title)
        };
        public PartOnePageInfoViewModel PageInfo { get; set; }
        public string FormPostRouteName => VacancyId.HasValue ? RouteNames.Title_Post : RouteNames.CreateVacancy_Post;
        public bool HasCloneableVacancies { get; set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public string BackLink =>
            HasCloneableVacancies ? RouteNames.Vacancies_Get : RouteNames.CreateVacancy_Get;
    }
}