using System;
using System.Linq;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.TagHelpers
{
    public interface IFieldReviewHelper
    {
        bool ShowReviewField(VacancyPreviewViewModel model, string fieldIdentifier);
        string GetReviewSectionClass(string sectionState);
    }
    public class FieldReviewHelper : IFieldReviewHelper
    {
        private const string EmptySectionClass = "app-preview-section--empty";
        private const string ErrorSectionClass = "app-preview-section--error";
        private const string ReviewSectionClass = "app-preview-section--review";
        
        public bool ShowReviewField(VacancyPreviewViewModel model, string fieldIdentifier)
        {
            if (model.Status != VacancyStatus.Rejected && model.Status != VacancyStatus.Referred)
                return false;
            
            return model.EmployerReviewFieldIndicators?.Any(p => p.FieldIdentifier == fieldIdentifier && p.IsChangeRequested) ?? false;
        }

        public string GetReviewSectionClass(string sectionState)
        {
            Enum.TryParse<VacancyPreviewSectionState>(sectionState, out var section);
            
            switch (section)
            {
                case VacancyPreviewSectionState.Valid:
                    return string.Empty;
                case VacancyPreviewSectionState.Incomplete:
                    return EmptySectionClass;
                case VacancyPreviewSectionState.Review:
                    return ReviewSectionClass;
                default:
                    return ErrorSectionClass;
            }
        }
    }
}