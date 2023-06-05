using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ShareMultipleApplicationsPostRequest
    {
        public List<Guid?> ApplicationsToShare { get; set; }
    }
}
