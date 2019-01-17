using System;
using System.Threading.Tasks;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Shared.Web.Services
{
    public interface IReviewSummaryService
    {
        Task<ReviewSummaryViewModel> GetReviewSummaryViewModelAsync(long vacancyReference,
            ReviewFieldMappingLookupsForPage fieldMappingsLookup);

        Task<ReviewSummaryViewModel> GetReviewSummaryViewModelAsync(Guid reviewId,
            ReviewFieldMappingLookupsForPage reviewFieldIndicatorsForPage);
    }
}