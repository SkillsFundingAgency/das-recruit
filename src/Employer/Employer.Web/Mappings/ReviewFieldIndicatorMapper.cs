using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.ShortDescription;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Employer.Web.Views;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FieldIdentifiers = Esfa.Recruit.Vacancies.Client.Domain.Entities.VacancyReview.FieldIdentifiers;

namespace Esfa.Recruit.Employer.Web.Mappings
{
    public static class ReviewFieldIndicatorMapper
    {
        public static readonly IReadOnlyList<ReviewFieldIndicatorViewModel> PreviewReviewFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            //These need to be added in display order
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Title, Anchors.Title, "Title requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ShortDescription, Anchors.ShortDescription, "Brief overview of the role requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ClosingDate, Anchors.ClosingDate, "Closing date requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.WorkingWeek, Anchors.WorkingWeek, "Working week requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Wage, Anchors.YearlyWage, "Yearly wage requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ExpectedDuration, Anchors.ExpectedDuration, "Expected duration requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.PossibleStartDate, Anchors.PossibleStartDate, "Possible start date requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.TrainingLevel, Anchors.TrainingLevel, "Apprenticeship level requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.NumberOfPositions, Anchors.NumberOfPositions, "Positions requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.VacancyDescription, Anchors.VacancyDescription, "Typical working day requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.TrainingDescription, Anchors.TrainingDescription, "Training requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.OutcomeDescription, Anchors.OutcomeDescription, "Future prospects requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Skills, Anchors.Skills, "Skills requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Qualifications, Anchors.Qualifications, "Qualifications requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ThingsToConsider, Anchors.ThingsToConsider, "Things to consider requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerDescription, Anchors.EmployerDescription, "Employer description requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.DisabilityConfident, Anchors.DisabilityConfident, "Disability confident requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerWebsiteUrl, Anchors.EmployerWebsiteUrl, "Employer website requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerContact, Anchors.EmployerContact, "Contact details requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerAddress, Anchors.EmployerAddress, "Employer address requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Provider, Anchors.Provider, "Training provider requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Training, Anchors.Training, "Training requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationMethod, Anchors.ApplicationMethod, "Application method requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationUrl, Anchors.ApplicationUrl, "Apply now web address requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationInstructions, Anchors.ApplicationInstructions, "Application process requires edit")
        };

        public static readonly IReadOnlyList<ReviewFieldIndicatorViewModel> ShortDescriptionReviewFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ShortDescription, nameof(ShortDescriptionEditModel.ShortDescription), "Brief overview of the role requires edit"),
        };

        public static readonly IReadOnlyList<ReviewFieldIndicatorViewModel> TrainingReviewFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ClosingDate, nameof(TrainingEditModel.ClosingDay), "Closing date requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.PossibleStartDate, nameof(TrainingEditModel.StartDay), "Possible start date requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Training, nameof(TrainingEditModel.SelectedProgrammeId), "Training requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.TrainingLevel, nameof(TrainingEditModel.SelectedProgrammeId), "Apprenticeship level requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.DisabilityConfident, nameof(TrainingEditModel.IsDisabilityConfident), "Disability confident requires edit"),
        };

        public static readonly IReadOnlyList<ReviewFieldIndicatorViewModel> WageReviewFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ExpectedDuration, nameof(WageEditModel.Duration), "How long is the apprenticeship expected to last requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.WorkingWeek, nameof(WageEditModel.WorkingWeekDescription), "Working week requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Wage, Anchors.WageTypeHeading, "What is the salary requires edit")
        };

        public static readonly IReadOnlyList<ReviewFieldIndicatorViewModel> TitleFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Title, nameof(TitleEditModel.Title), "What do you want to call this vacancy requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.NumberOfPositions, nameof(TitleEditModel.NumberOfPositions), "Number of positions for this apprenticeship requires edit")
        };

        public static readonly IReadOnlyList<ReviewFieldIndicatorViewModel> EmployerFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerAddress, nameof(EmployerEditModel.AddressLine1), "Employer address requires edit"),
        };

        public static readonly IReadOnlyList<ReviewFieldIndicatorViewModel> VacancyDescriptionFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.VacancyDescription, nameof(VacancyDescriptionEditModel.VacancyDescription), "What does the apprenticeship involve requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.TrainingDescription, nameof(VacancyDescriptionEditModel.TrainingDescription), "What training will your apprentice get requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.OutcomeDescription, nameof(VacancyDescriptionEditModel.OutcomeDescription), "What can the apprentice expect at the end of the apprenticeship requires edit"),
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
