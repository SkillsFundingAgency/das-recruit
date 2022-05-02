using System.Linq;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.TagHelpers
{
    public interface IFieldReviewHelper
    {
        bool ShowReviewField(VacancyPreviewViewModel model, string fieldIdentifier);
    }
    public class FieldReviewHelper : IFieldReviewHelper
    {
        public bool ShowReviewField(VacancyPreviewViewModel model, string fieldIdentifier)
        {
            if (model.Status != VacancyStatus.Rejected && model.Status != VacancyStatus.Referred)
                return false;
            
            return model.EmployerReviewFieldIndicators?.Any(p => p.FieldIdentifier == fieldIdentifier && p.IsChangeRequested) ?? false;
        }
    }
}