using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Employer.Web.Views;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FieldIdentifiers = Esfa.Recruit.Vacancies.Client.Domain.Entities.VacancyReview.FieldIdentifiers;

namespace Esfa.Recruit.Employer.Web.Mappings
{
    public static class ReviewFieldIndicatorMapper
    {
        public static readonly List<ReviewFieldIndicatorViewModel> PreviewReviewFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            //These need to be added in display order
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Title, PreviewAnchors.Title, "Title requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ShortDescription, PreviewAnchors.ShortDescription, "Brief overview of the role requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ClosingDate, PreviewAnchors.ClosingDate, "Closing date requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.WorkingWeek, PreviewAnchors.WorkingWeek, "Working week requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Wage, PreviewAnchors.YearlyWage, "Wage requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ExpectedDuration, PreviewAnchors.ExpectedDuration, "Expected duration requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.PossibleStartDate, PreviewAnchors.PossibleStartDate, "Possible start date requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.TrainingLevel, PreviewAnchors.TrainingLevel, "Apprenticeship level requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.NumberOfPositions, PreviewAnchors.NumberOfPositions, "Positions requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.VacancyDescription, PreviewAnchors.VacancyDescription, "Typical working day requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.TrainingDescription, PreviewAnchors.TrainingDescription, "Training requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.OutcomeDescription, PreviewAnchors.OutcomeDescription, "Future prospects requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Skills, PreviewAnchors.Skills, "Skills requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Qualifications, PreviewAnchors.Qualifications, "Qualifications requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ThingsToConsider, PreviewAnchors.ThingsToConsider, "Things to consider requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerDescription, PreviewAnchors.EmployerDescription, "Employer description requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.DisabilityConfident, PreviewAnchors.DisabilityConfident, "Disability confident requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerWebsiteUrl, PreviewAnchors.EmployerWebsiteUrl, "Employer website requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerContact, PreviewAnchors.EmployerContact, "Contact details requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerAddress, PreviewAnchors.EmployerAddress, "Employer address requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Provider, PreviewAnchors.Provider, "Training provider requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Training, PreviewAnchors.Training, "Training requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationMethod, PreviewAnchors.ApplicationMethod, "Application method requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationUrl, PreviewAnchors.ApplicationUrl, "Apply now web address requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationInstructions, PreviewAnchors.ApplicationInstructions, "Application process requires edit")
        };

        public static readonly List<ReviewFieldIndicatorViewModel> ShortDescriptionReviewFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ShortDescription, "ShortDescription", "Brief overview of the role requires edit"),
        };

        public static IEnumerable<ReviewFieldIndicatorViewModel> MapFromFieldIndicators(IEnumerable<ReviewFieldIndicatorViewModel> reviewFieldIndicatorsForPage, List<ManualQaFieldIndicator> reviewFieldIndicators)
        {
            var selectedFieldIdentifiers = reviewFieldIndicators
                .Where(r => r.IsChangeRequested)
                .Select(r => r.FieldIdentifier)
                .ToList();

            return reviewFieldIndicatorsForPage.Where(r => selectedFieldIdentifiers.Contains(r.ReviewFieldIdentifier));
        }
    }
}
