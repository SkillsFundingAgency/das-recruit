using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Humanizer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview
{
    public class VacancyPreviewViewModel(bool isMultipleLocationsEnabled = false) : DisplayVacancyViewModel 
    {
        public VacancyPreviewSectionState ApplicationInstructionsSectionState { get; internal set; }
        public VacancyPreviewSectionState ApplicationMethodSectionState { get; internal set; }
        public VacancyPreviewSectionState ApplicationUrlSectionState { get; internal set; }
        public VacancyPreviewSectionState ClosingDateSectionState { get; internal set; }
        public VacancyPreviewSectionState DisabilityConfidentSectionState { get; internal set; }
        public VacancyPreviewSectionState EmployerDescriptionSectionState { get; internal set; }
        public VacancyPreviewSectionState EmployerNameSectionState { get; internal set; }
        public VacancyPreviewSectionState EmployerWebsiteUrlSectionState { get; internal set; }
        public VacancyPreviewSectionState ExpectedDurationSectionState { get; internal set; }
        public VacancyPreviewSectionState EmployerAddressSectionState { get; internal set; }
        public VacancyPreviewSectionState NumberOfPositionsSectionState { get; internal set;}
        public VacancyPreviewSectionState PossibleStartDateSectionState { get; internal set; }
        public VacancyPreviewSectionState ProviderContactSectionState { get; internal set; }
        public VacancyPreviewSectionState ProviderSectionState { get; internal set; }
        public VacancyPreviewSectionState QualificationsSectionState { get; internal set; }
        public VacancyPreviewSectionState HasOptedToAddQualificationsSectionState { get; internal set; }
        public VacancyPreviewSectionState ShortDescriptionSectionState { get; internal set; }
        public VacancyPreviewSectionState SkillsSectionState { get; internal set; }
        public VacancyPreviewSectionState TitleSectionState { get; internal set; }
        public VacancyPreviewSectionState ThingsToConsiderSectionState { get; internal set; }
        public VacancyPreviewSectionState TrainingSectionState { get; internal set; }
        public VacancyPreviewSectionState TrainingLevelSectionState { get; internal set; }
        public VacancyPreviewSectionState WageTextSectionState { get; internal set; }
        public VacancyPreviewSectionState DescriptionsSectionState { get; internal set; }
        public VacancyPreviewSectionState WorkingWeekSectionState { get; internal set; }
        public VacancyPreviewSectionState FutureProspectsSectionState { get; internal set; }
        
        public VacancyPreviewSectionState TrainingDescriptionSectionState { get; internal set; }
        public VacancyPreviewSectionState VacancyDescriptionSectionState { get; internal set; }
        public VacancyPreviewSectionState WorkingWeekHoursSectionState { get; internal set; }
        public VacancyPreviewSectionState WorkingWeekDescriptionSectionState { get; internal set; }
        public VacancyPreviewSectionState WageAdditionalInfoSectionState { get; internal set; }
        public VacancyPreviewSectionState AdditionalQuestion1SectionState { get; internal set; }
        public VacancyPreviewSectionState AdditionalQuestion2SectionState { get; internal set; }

        public EntityValidationResult SoftValidationErrors { get; internal set; }
        public bool CanHideValidationSummary { get; internal set; }

        public bool HasWage { get; internal set; }
        public bool HasProgramme => !string.IsNullOrEmpty(TrainingTitle);
        public bool CanShowReference { get; set; }

        public bool HasIncompleteVacancyDescription => !HasVacancyDescription;
        public bool HasIncompleteShortDescription => !HasShortDescription;
        public bool CanShowDraftHeader { get; internal set; }

        public string InfoMessage { get; internal set; }

        public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);

        public bool RequiresEmployerReview { get; internal set; }

        public int IncompleteRequiredSectionCount => new[]
        {
            ShortDescriptionSectionState,
            SkillsSectionState,
            DescriptionsSectionState,
            QualificationsSectionState,
            EmployerDescriptionSectionState,
            TrainingSectionState,
            ApplicationMethodSectionState
        }.Count(s => s == VacancyPreviewSectionState.Incomplete || s == VacancyPreviewSectionState.InvalidIncomplete);

        public string IncompleteRequiredSectionText => "section".ToQuantity(IncompleteRequiredSectionCount, ShowQuantityAs.None);

        public bool HasIncompleteSkillsSection => SkillsSectionState == VacancyPreviewSectionState.Incomplete || SkillsSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteQualificationsSection => QualificationsSectionState == VacancyPreviewSectionState.Incomplete || QualificationsSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteEmployerDescriptionSection => EmployerDescriptionSectionState == VacancyPreviewSectionState.Incomplete || EmployerDescriptionSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteTrainingProviderSection => ProviderSectionState == VacancyPreviewSectionState.Incomplete || ProviderSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteApplicationProcessSection => ApplicationMethodSectionState == VacancyPreviewSectionState.Incomplete || ApplicationMethodSectionState == VacancyPreviewSectionState.InvalidIncomplete;

        public bool HasIncompleteThingsToConsiderSection => ThingsToConsiderSectionState == VacancyPreviewSectionState.Incomplete || ThingsToConsiderSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteEmployerWebsiteUrlSection => EmployerWebsiteUrlSectionState == VacancyPreviewSectionState.Incomplete || EmployerWebsiteUrlSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteProviderContactSection => ProviderContactSectionState == VacancyPreviewSectionState.Incomplete || ProviderContactSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteMandatorySections => HasIncompleteShortDescription
                                                      || HasIncompleteVacancyDescription
                                                      || HasIncompleteSkillsSection
                                                      || HasIncompleteQualificationsSection
                                                      || HasIncompleteEmployerDescriptionSection
                                                      || HasIncompleteTrainingProviderSection
                                                      || HasIncompleteApplicationProcessSection;
        public bool HasIncompleteOptionalSections => HasIncompleteThingsToConsiderSection
                                                    || HasIncompleteEmployerWebsiteUrlSection
                                                    || HasIncompleteProviderContactSection;
        public bool HasSoftValidationErrors => SoftValidationErrors?.HasErrors == true;

        public bool ShowIncompleteSections => ((HasIncompleteMandatorySections || HasIncompleteOptionalSections) && !Review.HasBeenReviewed) || HasSoftValidationErrors;
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public ValidationSummaryViewModel ValidationErrors { get; set; } = new ValidationSummaryViewModel(); 
        
        public string SubmitButtonText => RequiresEmployerReview && VacancyType.GetValueOrDefault() == Vacancies.Client.Domain.Entities.VacancyType.Apprenticeship
            ? Status == VacancyStatus.Rejected
                ? "Resubmit vacancy to employer"
                : "Send to employer"
            : Review.HasBeenReviewed
                ? "Resubmit vacancy"
                : "Submit vacancy";
        public bool ApplicationInstructionsRequiresEdit => IsEditRequired(FieldIdentifiers.ApplicationInstructions);
        public bool ApplicationMethodRequiresEdit => IsEditRequired(FieldIdentifiers.ApplicationMethod);
        public bool ApplicationUrlRequiresEdit => IsEditRequired(FieldIdentifiers.ApplicationUrl);
        public bool ClosingDateRequiresEdit => IsEditRequired(FieldIdentifiers.ClosingDate);
        public bool DisabilityConfidentRequiresEdit => IsEditRequired(FieldIdentifiers.DisabilityConfident);
        public bool EmployerAddressRequiresEdit => IsEditRequired(FieldIdentifiers.EmployerAddress);
        public bool EmployerNameRequiresEdit => IsEditRequired(FieldIdentifiers.EmployerName);
        public bool EmployerDescriptionRequiresEdit => IsEditRequired(FieldIdentifiers.EmployerDescription);
        public bool EmployerWebsiteUrlRequiresEdit => IsEditRequired(FieldIdentifiers.EmployerWebsiteUrl);
        public bool ExpectedDurationRequiresEdit => IsEditRequired(FieldIdentifiers.ExpectedDuration);
        public bool NumberOfPositionsRequiresEdit => IsEditRequired(FieldIdentifiers.NumberOfPositions);
        public bool OutcomeDescriptionRequiresEdit => IsEditRequired(FieldIdentifiers.OutcomeDescription);
        public bool PossibleStartDateRequiresEdit => IsEditRequired(FieldIdentifiers.PossibleStartDate);
        public bool ProviderContactRequiresEdit => IsEditRequired(FieldIdentifiers.ProviderContact);
        public bool ProviderRequiresEdit => IsEditRequired(FieldIdentifiers.Provider);
        public bool QualificationsRequiresEdit => IsEditRequired(FieldIdentifiers.Qualifications);
        public bool ShortDescriptionRequiresEdit => IsEditRequired(FieldIdentifiers.ShortDescription);
        public bool SkillsRequiresEdit => IsEditRequired(FieldIdentifiers.Skills);
        public bool ThingsToConsiderRequiresEdit => IsEditRequired(FieldIdentifiers.ThingsToConsider);
        public bool TitleRequiresEdit => IsEditRequired(FieldIdentifiers.Title);
        public bool TrainingRequiresEdit => IsEditRequired(FieldIdentifiers.Training);
        public bool TrainingDescriptionRequiresEdit => IsEditRequired(FieldIdentifiers.TrainingDescription);
        public bool TrainingLevelRequiresEdit => IsEditRequired(FieldIdentifiers.TrainingLevel);
        public bool VacancyDescriptionRequiresEdit => IsEditRequired(FieldIdentifiers.VacancyDescription);
        public bool WageRequiresEdit => IsEditRequired(FieldIdentifiers.Wage);
        public bool WorkingWeekRequiresEdit => IsEditRequired(FieldIdentifiers.WorkingWeek);
        public bool HasSelectedEmployerNameOption => EmployerNameOption != null;
        private bool IsEditRequired(string fieldIdentifier)
        {
            return Review.FieldIndicators.Any(f => f.ReviewFieldIdentifier == fieldIdentifier);
        }

        public bool HasUserConfirmation { get; set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ShortDescription),
            nameof(ClosingDate),
            nameof(WorkingWeekDescription),
            nameof(HasWage),
            nameof(WorkingWeekDescription),
            nameof(HoursPerWeek),
            nameof(WageText),
            nameof(WageInfo),
            nameof(CompanyBenefitsInformation),
            nameof(ExpectedDuration),
            nameof(PossibleStartDate),
            nameof(HasProgramme),
            nameof(TrainingLevel),
            nameof(NumberOfPositions),
            nameof(VacancyDescription),
            nameof(TrainingDescription),
            nameof(AdditionalTrainingDescription),
            nameof(OutcomeDescription),
            nameof(Skills),
            nameof(Qualifications),
            nameof(ThingsToConsider),
            nameof(EmployerDescription),
            nameof(EmployerName),
            nameof(EmployerWebsiteUrl),
            nameof(EmployerAddressElements),
            nameof(ApplicationMethod),
            nameof(ApplicationInstructions),
            nameof(ApplicationUrl),
            nameof(ProviderName),
            nameof(ProviderContactName),
            nameof(ProviderContactEmail),
            nameof(ProviderContactTelephone),
            nameof(TrainingType),
            nameof(TrainingTitle)
        };
        
        public void SetSectionStates(VacancyPreviewViewModel viewModel, ModelStateDictionary modelState)
        {
            viewModel.TitleSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.Title }, true, modelState, vm => vm.Title);
            viewModel.ShortDescriptionSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ShortDescription }, true, modelState, vm => vm.ShortDescription);
            viewModel.ClosingDateSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ClosingDate }, true, modelState, vm => vm.ClosingDate);
            viewModel.WorkingWeekSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.WorkingWeek }, true, modelState, vm => vm.HoursPerWeek, vm => vm.WorkingWeekDescription);
            viewModel.WorkingWeekHoursSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.WorkingWeekHours }, true, modelState, vm => vm.HoursPerWeek);
            viewModel.WorkingWeekDescriptionSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.WorkingWeekDescription }, true, modelState, vm => vm.WorkingWeekDescription);
            viewModel.WageTextSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.Wage }, true, modelState, vm => vm.HasWage, vm => vm.WageText);
            viewModel.WageAdditionalInfoSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.WageAdditionalInfo }, true, modelState, vm => vm.WageText);
            viewModel.ExpectedDurationSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ExpectedDuration }, true, modelState, vm => vm.ExpectedDuration);
            viewModel.PossibleStartDateSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.PossibleStartDate }, true, modelState, vm => vm.PossibleStartDate);
            viewModel.TrainingLevelSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.TrainingLevel }, true, modelState, vm => vm.HasProgramme, vm => vm.TrainingLevel);
            viewModel.NumberOfPositionsSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.NumberOfPositions }, true, modelState, vm => vm.NumberOfPositions);
            viewModel.DescriptionsSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.VacancyDescription }, true, modelState,vm => vm.VacancyDescription);
            viewModel.VacancyDescriptionSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.VacancyDescription }, true, modelState, vm => vm.VacancyDescription);
            viewModel.TrainingDescriptionSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.TrainingDescription }, true, modelState, vm => vm.TrainingDescription);
            viewModel.FutureProspectsSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.OutcomeDescription }, true, modelState,  vm => vm.OutcomeDescription);
            viewModel.SkillsSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.Skills }, true, modelState, vm => vm.Skills);
            viewModel.QualificationsSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.Qualifications }, false, modelState, vm => vm.Qualifications, vm => vm.QualificationsDesired);
            viewModel.HasOptedToAddQualificationsSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.HasOptedToAddQualifications }, true, modelState,vm => vm.HasOptedToAddQualifications);
            viewModel.ThingsToConsiderSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ThingsToConsider }, true, modelState, vm => vm.ThingsToConsider);
            viewModel.EmployerNameSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.EmployerName }, true, modelState, vm => vm.EmployerName);
            viewModel.EmployerDescriptionSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.EmployerDescription }, true, modelState, vm => vm.EmployerDescription);
            viewModel.EmployerWebsiteUrlSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.EmployerWebsiteUrl }, true, modelState, vm => vm.EmployerWebsiteUrl);
            
            // We're only validating against the multiple addresses field if the vacancy has gone through the
            // new pipeline (indicated by AvailableWhere being set), otherwise it will be an existing record
            // that used the previous single location selection page.
            if (isMultipleLocationsEnabled && viewModel.AvailableWhere.HasValue)
            {
                viewModel.EmployerAddressSectionState = viewModel.AvailableWhere switch
                {
                    Recruit.Vacancies.Client.Domain.Entities.AvailableWhere.AcrossEngland => GetSectionState(viewModel, [FieldIdentifiers.EmployerAddress], true, modelState, vm => vm.LocationInformation),
                    _ => GetSectionState(viewModel, [FieldIdentifiers.EmployerAddress], true, modelState, vm => vm.AvailableLocations),
                };
            }
            else
            {
                viewModel.EmployerAddressSectionState = GetSectionState(viewModel, [FieldIdentifiers.EmployerAddress], true, modelState, vm => vm.EmployerAddressElements);
            }
            
            viewModel.ApplicationInstructionsSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ApplicationInstructions }, true, modelState, vm => vm.ApplicationInstructions);
            viewModel.ApplicationMethodSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ApplicationMethod }, true, modelState, vm => vm.ApplicationMethod);
            viewModel.ApplicationUrlSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ApplicationUrl }, true, modelState, vm => vm.ApplicationUrl);
            viewModel.ProviderSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.Provider }, true, modelState, vm => vm.ProviderName);
            viewModel.ProviderContactSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ProviderContact }, false, modelState, vm => vm.ProviderContactName, vm => vm.ProviderContactEmail, vm => vm.ProviderContactTelephone);
            viewModel.TrainingSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.Training }, true, modelState, vm => vm.TrainingType, vm => vm.TrainingTitle);
            viewModel.DisabilityConfidentSectionState = GetSectionState(viewModel, new[]{ FieldIdentifiers.DisabilityConfident}, true, modelState, vm => vm.IsDisabilityConfident);
            viewModel.AdditionalQuestion1SectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.AdditionalQuestion1 }, true, modelState, vm => vm.AdditionalQuestion1SectionState);
            viewModel.AdditionalQuestion2SectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.AdditionalQuestion2 }, true, modelState, vm => vm.AdditionalQuestion2SectionState);
        }

        public int AccountCount { get; set; }
        public int AccountLegalEntityCount { get ; set ; }

        public VacancyTaskListSectionState TaskListSectionOneState => SetTaskListSectionState();

        public VacancyTaskListSectionState TaskListSectionTwoState => SetTaskListSectionTwoState();
        public VacancyTaskListSectionState TaskListSectionThreeState => SetTaskListSectionThreeState();
        public VacancyTaskListSectionState TaskListSectionFourState => SetTaskListSectionFourState();
        public VacancyTaskListSectionState TaskListSectionFiveState => SetTaskListSectionFiveState();
        public string AccountId { get; set; }
        public bool CanShowVacancyClonedStatusHeader => !string.IsNullOrEmpty(VacancyClonedInfoMessage);
        public string VacancyClonedInfoMessage { get; set; }
        
        private VacancyTaskListSectionState SetTaskListSectionState()
        {
            if (TitleSectionState == VacancyPreviewSectionState.Valid
                && HasProgramme
                && !string.IsNullOrEmpty(Title)
                && HasSelectedLegalEntity
                && HasShortDescription
                && DescriptionsSectionState == VacancyPreviewSectionState.Valid
                && HasVacancyDescription)
            {
                return VacancyTaskListSectionState.Completed;
            }
            
            if (HasSelectedLegalEntity)
            {
                return VacancyTaskListSectionState.InProgress;    
            }
            
            return  VacancyTaskListSectionState.NotStarted;
            
        }
        private VacancyTaskListSectionState SetTaskListSectionTwoState()
        {
            if (TaskListSectionOneState != VacancyTaskListSectionState.Completed)
            {
                return VacancyTaskListSectionState.NotStarted;
            } 
            
            if ((WageTextSectionState == VacancyPreviewSectionState.Valid)
                && ExpectedDurationSectionState == VacancyPreviewSectionState.Valid
                && ClosingDateSectionState == VacancyPreviewSectionState.Valid
                && PossibleStartDateSectionState == VacancyPreviewSectionState.Valid
                && NumberOfPositionsSectionState == VacancyPreviewSectionState.Valid
                && EmployerAddressSectionState == VacancyPreviewSectionState.Valid)
            {
                return VacancyTaskListSectionState.Completed;
            }
            
            if (HasPossibleStartDate && HasClosingDate)
            {
                return VacancyTaskListSectionState.InProgress;
            }
            return VacancyTaskListSectionState.NotStarted;
        }
        private VacancyTaskListSectionState SetTaskListSectionThreeState()
        {
            if (TaskListSectionTwoState != VacancyTaskListSectionState.Completed)
            {
                return VacancyTaskListSectionState.NotStarted;
            } 
            
            if (HasSkills 
                && CheckQualificationSectionStatus() 
                && HasOutcomeDescription)
            {
                return VacancyTaskListSectionState.Completed;
            }

            if (HasSkills)
            {
                return VacancyTaskListSectionState.InProgress;
            }
            return VacancyTaskListSectionState.NotStarted;
        }
        
        private bool CheckQualificationSectionStatus()
        {
            return (HasOptedToAddQualifications is false || (HasOptedToAddQualifications is true 
                                                             && QualificationsSectionState == VacancyPreviewSectionState.Valid));
        }
        
        private VacancyTaskListSectionState SetTaskListSectionFourState()
        {
            if (TaskListSectionThreeState != VacancyTaskListSectionState.Completed)
            {
                return VacancyTaskListSectionState.NotStarted;
            }
            
            if (TaskListSectionThreeState == VacancyTaskListSectionState.Completed 
                && !HasSelectedEmployerNameOption)
            {
                return VacancyTaskListSectionState.NotStarted;
            }
            
            if (HasSelectedEmployerNameOption
                && ApplicationMethodSectionState == VacancyPreviewSectionState.Valid
                && EmployerDescriptionSectionState == VacancyPreviewSectionState.Valid)
            {
                return VacancyTaskListSectionState.Completed;
            }
            
            return VacancyTaskListSectionState.InProgress;
        }
        
        private VacancyTaskListSectionState SetTaskListSectionFiveState()
        {
            if (TaskListSectionFourState != VacancyTaskListSectionState.Completed)
            {
                return VacancyTaskListSectionState.NotStarted;
            }

            if (HasSubmittedAdditionalQuestions
                && AdditionalQuestion1SectionState == VacancyPreviewSectionState.Valid
                && AdditionalQuestion2SectionState == VacancyPreviewSectionState.Valid)
            {
                return VacancyTaskListSectionState.Completed;
            }

            return VacancyTaskListSectionState.NotStarted;
        }
        
        private VacancyPreviewSectionState GetSectionState(VacancyPreviewViewModel vm, IEnumerable<string> reviewFieldIndicators, bool requiresAll, ModelStateDictionary modelState,  params Expression<Func<VacancyPreviewViewModel, object>>[] sectionProperties)
        {
            if (IsSectionModelStateValid(modelState, sectionProperties) == false)
            {
                return IsSectionComplete(vm, requiresAll, sectionProperties) ? 
                    VacancyPreviewSectionState.Invalid : 
                    VacancyPreviewSectionState.InvalidIncomplete;
            }

            if (IsSectionForReview(vm, reviewFieldIndicators))
                return VacancyPreviewSectionState.Review;

            return IsSectionComplete(vm, requiresAll, sectionProperties) ?
                VacancyPreviewSectionState.Valid :
                VacancyPreviewSectionState.Incomplete;
        }

        private bool IsSectionModelStateValid(ModelStateDictionary modelState, params Expression<Func<VacancyPreviewViewModel, object>>[] sectionProperties)
        {
            if (modelState.IsValid)
                return true;

            foreach (var property in sectionProperties)
            {
                var propertyName = property.GetPropertyName();
                if (modelState.Keys.Any(k => k == propertyName && modelState[k].Errors.Any()))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsSectionComplete(VacancyPreviewViewModel vm, bool requiresAll, params Expression<Func<VacancyPreviewViewModel, object>>[] sectionProperties)
        {
            foreach (var requiredPropertyExpression in sectionProperties)
            {
                var requiredPropertyFunc = requiredPropertyExpression.Compile();
                var propertyValue = requiredPropertyFunc(vm);

                var result = true;
                switch (propertyValue)
                {
                    case null:
                        result = false;
                        break;
                    case string stringProperty:
                        if (string.IsNullOrWhiteSpace(stringProperty))
                        {
                            result = false;
                        }
                        break;
                    case IEnumerable listProperty:
                        if (listProperty.Cast<object>().Any() == false)
                        {
                            result = false;
                        }
                        break;
                    case bool _:
                        //No way to tell if a bool has been 'completed' so just skip
                        break;
                    default:
                        //Skipping other types for now
                        break;
                }

                if (requiresAll && result == false)
                {
                    return false;
                }

                if (!requiresAll && result)
                {
                    return true;
                }
            }

            return requiresAll;
        }

        private bool IsSectionForReview(VacancyPreviewViewModel vm, IEnumerable<string> reviewFieldIndicators)
        {
            return reviewFieldIndicators != null && reviewFieldIndicators.Any(reviewFieldIndicator =>
                       vm.Review.FieldIndicators.Select(r => r.ReviewFieldIdentifier)
                           .Contains(reviewFieldIndicator));
        }

        
        
    }

    public enum VacancyPreviewSectionState
    {
        Incomplete,
        Valid,
        Invalid,
        InvalidIncomplete,
        Review
    }
    public enum VacancyTaskListSectionState
    {
        NotStarted,
        InProgress,
        Completed
    }
}

