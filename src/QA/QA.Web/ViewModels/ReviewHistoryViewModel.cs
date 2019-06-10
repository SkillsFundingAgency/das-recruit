using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class ReviewHistoriesViewModel
    {
        public IEnumerable<ReviewHistoryViewModel> Items { get; set; }

        public bool HasHistories => Items.Any();
    }

    public class ReviewHistoryViewModel
    {
        public string ReviewerName { get; set; }
        public Guid ReviewId { get; set; }
        public string Outcome { get; set; }
        public DateTime ReviewDate { get; set; }

        public string ReviewDateDay => ReviewDate.ToUkTime().AsGdsDate();
        public string ReviewDateTime => ReviewDate.ToUkTime().AsGdsTime();
    }
}
