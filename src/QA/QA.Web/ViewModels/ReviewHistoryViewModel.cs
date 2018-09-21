using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class ReviewHistoriesViewModel
    {
        public IEnumerable<ReviewHistoryViewModel> Items { get; set; }
    }

    public class ReviewHistoryViewModel
    {
        public string ReviewerName { get; set; }
        public Guid ReviewId { get; set; }
        public string Outcome { get; set; }
        public DateTime ReviewDate { get; set; }

        public string ReviewDateDay => ReviewDate.ToLocalTime().AsGdsDate();
        public string ReviewDateTime => ReviewDate.ToLocalTime().AsGdsTime();
    }
}
