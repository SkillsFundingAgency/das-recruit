using System;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.TagHelpers
{
    public interface IFieldReviewHelper
    {
        string GetReviewSectionClass(string sectionState);
        bool ShowReviewField(VacancyPreviewViewModel model, string fieldIdentifier);
    }
    
    public class FieldReviewHelper : IFieldReviewHelper
    {
        private const string EmptySectionClass = "app-check-answers__key--empty";
        private const string ErrorSectionClass = "app-check-answers__key--error";
        private const string ReviewSectionClass = "app-check-answers__key--review";
        
        public bool ShowReviewField(VacancyPreviewViewModel model, string fieldIdentifier)
        {
            if (model.Status != VacancyStatus.Rejected && model.Status != VacancyStatus.Referred)
                return false;
            
            return model.ProviderReviewFieldIndicators?.Any(p => p.FieldIdentifier == fieldIdentifier && p.IsChangeRequested) ?? false;
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