﻿using System;
using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Title
{
    public class TitleViewModel
    {
        [FromRoute]
        public Guid? VacancyId { get; set; }
        private string _employerAccountId;

        [FromRoute]
        public string EmployerAccountId
        {
            get => _employerAccountId;
            set => _employerAccountId = value.ToUpper();
        }
        public string Title { get; set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(Title)
        };
        public string FormPostRouteName => VacancyId.HasValue ? RouteNames.Title_Post : RouteNames.CreateVacancy_Post;
        public PartOnePageInfoViewModel PageInfo { get; set; }
        public bool ReferredFromMa { get; set; }
        public string ReferredUkprn { get; set; }
        public string ReferredProgrammeId { get; set; }
        public bool ReferredFromSavedFavourites => ReferredFromMa &
                                                   (!string.IsNullOrEmpty(ReferredUkprn) ||
                                                    !string.IsNullOrEmpty(ReferredProgrammeId));
        public string TrainingTitle { get; set; }
        
        public Dictionary<string, string> RouteDictionary
        {
            get
            {
                var routeDictionary = new Dictionary<string, string>
                {
                    {"EmployerAccountId", EmployerAccountId}
                };
                if(VacancyId.HasValue)
                {
                    routeDictionary.Add("VacancyId", VacancyId.ToString());
                }
                return routeDictionary;
            }
        }
    }
}
