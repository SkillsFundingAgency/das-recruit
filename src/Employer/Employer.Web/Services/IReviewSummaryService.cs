using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface IReviewSummaryService
    {
        Task<ReviewSummaryViewModel> GetReviewSummaryViewModel(long vacancyReference, ReviewFieldMappingLookupsForPage fieldMappingsLookup);
    }
}