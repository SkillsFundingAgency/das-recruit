using System;
using Esfa.Recruit.Employer.Web.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.QueryStringModels
{
    public class EditDatesQueryStringModel
    {
        [FromQuery(Name = QueryString.ParameterNames.ProposedClosingDate)]
        public string ProposedClosingDate { get; internal set; }

        [FromQuery(Name = QueryString.ParameterNames.ProposedStartDate)]
        public string ProposedStartDate { get; internal set; }
    }
}
