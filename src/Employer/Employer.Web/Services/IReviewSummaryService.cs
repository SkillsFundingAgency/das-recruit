using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface IReviewSummaryService
    {
        Task<ReviewSummaryViewModel> GetReviewSummaryViewModel(long vacancyReference, IEnumerable<ReviewFieldIndicatorViewModel> reviewFieldIndicatorsForPage);
    }
}