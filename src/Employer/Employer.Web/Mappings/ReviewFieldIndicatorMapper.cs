using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.Services;
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

        public static readonly IReadOnlyList<ReviewFieldIndicatorViewModel> SkillsFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Skills, Anchors.SkillsHeading, "Desired skills or personal qualities requires edit"),
        };

        public static readonly IReadOnlyList<ReviewFieldIndicatorViewModel> QualificationsFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Qualifications, Anchors.QualificationsHeading, "Qualifications requires edit"),
        };

        public static readonly IReadOnlyList<ReviewFieldIndicatorViewModel> ConsiderationsFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ThingsToConsider, nameof(ConsiderationsEditModel.ThingsToConsider), "Things to consider requires edit"),
        };

        public static readonly IReadOnlyList<ReviewFieldIndicatorViewModel> AboutEmployerFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerDescription, nameof(AboutEmployerEditModel.EmployerDescription), "Tell us about your organisation requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerWebsiteUrl, nameof(AboutEmployerEditModel.EmployerWebsiteUrl), "Your organisation's website requires edit")
        };

        public static readonly IReadOnlyList<ReviewFieldIndicatorViewModel> EmployerContactDetailsFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerContact, nameof(EmployerContactDetailsEditModel.EmployerContactName), "Contact details requires edit"),
        };

        public static readonly IReadOnlyList<ReviewFieldIndicatorViewModel> TrainingProviderFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.Provider, nameof(SelectTrainingProviderEditModel.Ukprn), "Training provider requires edit"),
        };

        public static readonly IReadOnlyList<ReviewFieldIndicatorViewModel> ApplicationProcessFieldIndicators = new List<ReviewFieldIndicatorViewModel>
        {
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationMethod, Anchors.ApplicationMethodHeading, "Application process requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationUrl, nameof(ApplicationProcessEditModel.ApplicationUrl), "Enter the web address candidates should use to apply for this vacancy requires edit"),
            new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationInstructions, nameof(ApplicationProcessEditModel.ApplicationInstructions), "Explain the application process requires edit")
        };

        public static IEnumerable<ReviewFieldIndicatorViewModel> MapFromFieldIndicators(IEnumerable<ReviewFieldIndicatorViewModel> reviewFieldIndicatorsForPage, List<ManualQaFieldIndicator> reviewFieldIndicators, RuleSetOutcome automatedResult)
        {
            var selectedFieldIdentifiers = reviewFieldIndicators
                .Where(r => r.IsChangeRequested)
                .Select(r => r.FieldIdentifier)
                .ToList();

            var allLeafOutcomes = new List<RuleOutcome>();

            GetFailureLeafOutcomes(automatedResult.RuleOutcomes, allLeafOutcomes);

            var manualIndicatorsLookup = reviewFieldIndicatorsForPage.Where(r => selectedFieldIdentifiers.Contains(r.ReviewFieldIdentifier)).ToDictionary(x => x.ReviewFieldIdentifier, y => y);

            foreach(var outcome in allLeafOutcomes)
            {
                if (manualIndicatorsLookup.ContainsKey(outcome.Target))
                {
                    manualIndicatorsLookup[outcome.Target].Texts.Add(outcome.Narrative);
                }
                else
                {
                    var matchingField = reviewFieldIndicatorsForPage.SingleOrDefault(x => x.ReviewFieldIdentifier == outcome.Target);

                    if (matchingField != null) // The field might not be on the current page.
                    {
                        matchingField.Texts[0] = outcome.Narrative; // Override default text as manual message not needed.
                        manualIndicatorsLookup.Add(outcome.Target, reviewFieldIndicatorsForPage.Single(x => x.ReviewFieldIdentifier == outcome.Target));
                    }
                }
            }

            return manualIndicatorsLookup.Values;
        }

        public static void GetFailureLeafOutcomes(IEnumerable<RuleOutcome> ruleSetOutcome, IList<RuleOutcome> leafOutcomes)
        {
            foreach(var outcome in ruleSetOutcome)
            {
                if (outcome.Details == null || outcome.Details.Count() == 0)
                {
                    if (outcome.Score > 0)
                        leafOutcomes.Add(outcome);
                }
                else
                {
                    GetFailureLeafOutcomes(outcome.Details, leafOutcomes);
                }
            }
        }
    }
}
