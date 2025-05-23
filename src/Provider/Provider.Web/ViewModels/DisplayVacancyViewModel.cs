﻿using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using AvailableWhereType = Esfa.Recruit.Vacancies.Client.Domain.Entities.AvailableWhere;

namespace Esfa.Recruit.Provider.Web.ViewModels
{
    public abstract class DisplayVacancyViewModel : VacancyRouteModel
    {
        public VacancyStatus Status { get; set; }
        public string AccountName { get; set; }
        public string ApplicationInstructions { get; internal set; }
        public ApplicationMethod? ApplicationMethod { get; internal set; }
        public string ApplicationUrl { get; internal set; }
        public bool CanDelete { get; internal set; }
        public bool CanSubmit { get; internal set; }
        public bool IsSentForReview { get; internal set; }
        public string ClosingDate { get; internal set; }
        public string PostedDate { get; internal set; }
        public string EducationLevelName { get; internal set; }
        public string EmployerDescription { get; internal set; }
        public string EmployerName { get; internal set; }
        public string EmployerRejectedReason { get; internal set; }
        public List<EmployerReviewFieldIndicator> EmployerReviewFieldIndicators { get; internal set; }
        public string EmployerWebsiteUrl { get; internal set; }
        public string ExpectedDuration { get; internal set; }
        public string FindAnApprenticeshipUrl { get; internal set; }
        public string HoursPerWeek { get; internal set; }
        public bool IsAnonymous { get; internal set; }
        public bool IsDisabilityConfident { get; internal set; }
        public Address Location { get; internal set; }
        public IEnumerable<string> EmployerAddressElements { get; internal set; }
        public AvailableWhere? AvailableWhere { get; internal set; }
        public IEnumerable<Address> AvailableLocations { get; internal set; }
        public string? LocationInformation { get; internal set; }
        public string MapUrl { get; internal set; }
        public string NumberOfPositions { get; internal set; }
        public string NumberOfPositionsCaption { get; internal set; }
        public string OrganisationName { get; internal set; }
        public string OutcomeDescription { get; internal set; }
        public string PossibleStartDate { get; internal set; }
        public string ProviderContactName { get; internal set; }
        public string ProviderContactEmail { get; internal set; }
        public string ProviderContactTelephone { get; internal set; }
        public string ProviderName { get; internal set; }
        public List<string>? Qualifications { get; internal set; }
        public bool? HasOptedToAddQualifications { get; internal set; }
        public List<string>? QualificationsDesired { get; set; }
        public string ShortDescription { get; internal set; }
        public IEnumerable<string> Skills { get; internal set; }
        public string ThingsToConsider { get; internal set; }
        public string Title { get; internal set; }
        public string TrainingDescription { get; internal set; }
        public string AdditionalTrainingDescription { get; internal set; }
        public string TrainingTitle { get; internal set; }
        public List<string> CourseCoreDuties { get; internal set; } = [];
        public List<string> CourseSkills { get; internal set; } = [];
        public string TrainingType { get; internal set; }
        public string TrainingLevel { get; internal set; }
        public string VacancyDescription { get; internal set; }
        public string VacancyReferenceNumber { get; internal set; }
        public string WageInfo { get; internal set; }
        public string WageText { get; internal set; }
        public string CompanyBenefitsInformation { get; internal set; }
        public WageType? WageType { get; internal set; }
        public string WorkingWeekDescription { get; internal set; }
        public bool HasCompetitiveSalaryType => WageType.HasValue && WageType.Value == Vacancies.Client.Domain.Entities.WageType.CompetitiveSalary;
        public string AccountLegalEntityPublicHashedId { get; internal set; }
        public int RouteId { get; set; }
        public string RouteTitle { get; set; }
        private string _additionalQuestion1;
        public string AdditionalQuestion1 
        { 
            get { return BuildAdditionalQuestionText(_additionalQuestion1); }
            set { _additionalQuestion1 = value; }
        }
        private string _additionalQuestion2;
        public string AdditionalQuestion2 
        { 
            get { return BuildAdditionalQuestionText(_additionalQuestion2); }
            set { _additionalQuestion2 = value; }
        }
        public bool HasSubmittedAdditionalQuestions { get; internal set; }

        public bool HasClosingDate => !string.IsNullOrWhiteSpace(ClosingDate);

        public bool HasShortDescription => !string.IsNullOrWhiteSpace(ShortDescription);

        public bool HasProviderName => !string.IsNullOrWhiteSpace(ProviderName);

        public bool HasTrainingTitle => !string.IsNullOrWhiteSpace(TrainingTitle);

        public bool HasWorkingWeekDescription => !string.IsNullOrWhiteSpace(WorkingWeekDescription);

        public bool HasHoursPerWeek => !string.IsNullOrWhiteSpace(HoursPerWeek);

        public bool HasWageText => !string.IsNullOrWhiteSpace(WageText);

        public bool HasWageInfo => !string.IsNullOrWhiteSpace(WageInfo);

        public bool HasExpectedDuration => !string.IsNullOrWhiteSpace(ExpectedDuration);

        public bool HasPossibleStartDate => !string.IsNullOrWhiteSpace(PossibleStartDate);

        public bool HasTrainingLevel => !string.IsNullOrWhiteSpace(TrainingLevel);

        public bool HasNumberOfPositionsCaption => !string.IsNullOrWhiteSpace(NumberOfPositionsCaption);
        public bool HasMapUrl => !string.IsNullOrEmpty(MapUrl);

        public bool HasVacancyDescription => !string.IsNullOrWhiteSpace(VacancyDescription);

        public bool HasTrainingDescription => !string.IsNullOrWhiteSpace(TrainingDescription);

        public bool HasOutcomeDescription => !string.IsNullOrWhiteSpace(OutcomeDescription);

        public bool HasSkills => Skills != null && Skills.Any();

        public bool HasQualifications => Qualifications != null && Qualifications.Any();

        public bool HasThingsToConsider => !string.IsNullOrWhiteSpace(ThingsToConsider);

        public bool HasEmployerName => !string.IsNullOrWhiteSpace(EmployerName);

        public bool HasEmployerDescription => !string.IsNullOrWhiteSpace(EmployerDescription);

        public bool HasEmployerWebsiteUrl => !string.IsNullOrWhiteSpace(EmployerWebsiteUrl);

        public bool HasProviderContactName => !string.IsNullOrWhiteSpace(ProviderContactName);

        public bool HasProviderContactEmail => !string.IsNullOrWhiteSpace(ProviderContactEmail);

        public bool HasProviderContactTelephone => !string.IsNullOrWhiteSpace(ProviderContactTelephone);

        public bool HasEmployerAddressElements => EmployerAddressElements != null && EmployerAddressElements.Any();
        public bool HasNotSpecifiedApplicationMethod => !ApplicationMethod.HasValue;
        public bool HasApplicationMethod => ApplicationMethod.HasValue;
        public bool HasSpecifiedThroughFaaApplicationMethod => HasApplicationMethod && ApplicationMethod.Value == Esfa.Recruit.Vacancies.Client.Domain.Entities.ApplicationMethod.ThroughFindAnApprenticeship;

        public bool HasSpecifiedThroughFaTApplicationMethod => HasApplicationMethod && ApplicationMethod.Value == Vacancies.Client.Domain.Entities.ApplicationMethod.ThroughFindATraineeship;
        public bool HasSpecifiedThroughExternalApplicationMethod => HasApplicationMethod && ApplicationMethod.Value == Esfa.Recruit.Vacancies.Client.Domain.Entities.ApplicationMethod.ThroughExternalApplicationSite;
        public bool HasApplicationInstructions => !string.IsNullOrWhiteSpace(ApplicationInstructions);
        public bool HasApplicationUrl => !string.IsNullOrWhiteSpace(ApplicationUrl);
        public bool HasAdditionalQuestion1 => !string.IsNullOrWhiteSpace(AdditionalQuestion1);
        public bool HasAdditionalQuestion2 => !string.IsNullOrWhiteSpace(AdditionalQuestion2);

        public bool ShowGeneralApplicationProcessSectionTitle => ApplicationMethod == null || ApplicationMethod.Value != Esfa.Recruit.Vacancies.Client.Domain.Entities.ApplicationMethod.ThroughExternalApplicationSite;

        public bool IsNotDisabilityConfident => !IsDisabilityConfident;
        public bool HasSelectedLegalEntity => !string.IsNullOrEmpty(AccountLegalEntityPublicHashedId);
        public EmployerNameOption? EmployerNameOption { get; set; }
        public string StandardPageUrl { get; set; }
        public string OverviewOfRole { get; set; }
        public ApprenticeshipLevel ApprenticeshipLevel { get; set; }
        public ApprenticeshipTypes ApprenticeshipType { get; set; }
        
        private string BuildAdditionalQuestionText(string additionalQuestion)
        {
            if (string.IsNullOrWhiteSpace(additionalQuestion))
                return null;
                
            if (!additionalQuestion.EndsWith("?"))
                return additionalQuestion.TrimEnd() + "?";
                
            return additionalQuestion;
        }
        
        public string GetLocationDescription()
        {
            switch (AvailableWhere)
            {
                case AvailableWhereType.AcrossEngland:
                    {
                        return "Recruiting nationally";
                    }
                case AvailableWhereType.OneLocation:
                    {
                        var location = AvailableLocations.First();
                        return IsAnonymous
                            ? location.ToSingleLineAnonymousAddress()
                            : location.ToSingleLineAbridgedAddress();
                    }
                case AvailableWhereType.MultipleLocations:
                    {
                        var groupedAddresses = AvailableLocations.ToList().GroupByLastFilledAddressLine().ToList();
                        if (groupedAddresses is { Count: 1 })
                        {
                            int groupCount = groupedAddresses[0].Count();
                            if (groupCount > 1)
                            {
                                return $"{groupedAddresses[0].Key} ({groupCount} available locations)";
                            }
                        }

                        var keys = groupedAddresses.Select(group => group.Key);
                        return string.Join(", ", keys);
                    }
                default:
                    {
                        // This is for existing data that uses the old fields
                        return $"{EmployerAddressElements.SkipLast(1).LastOrDefault()} ({EmployerAddressElements.LastOrDefault()})";
                    }
            }
        }
    }
}
