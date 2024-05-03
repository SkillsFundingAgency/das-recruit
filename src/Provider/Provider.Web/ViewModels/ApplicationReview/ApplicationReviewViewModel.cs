using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview
{
    public class ApplicationReviewViewModel : VacancyRouteModel
    {
        public string VacancyTitle { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public ApplicationReviewDisabilityStatus DisabilityStatus { get; set; }
        public int EducationFromYear { get; set; }
        public string EducationInstitution { get; set; }
        public int EducationToYear { get; set; }
        public string Email { get; set; }
        public string FriendlyId { get; set; }
        public string Improvements { get; set; }
        public string HobbiesAndInterests { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Postcode { get; set; }
        public List<QualificationViewModel> Qualifications { get; set; }
        public List<QualificationGroup> QualificationGroups => BuildQualificationGroups();
        public List<string> Skills { get; set; }
        public ApplicationReviewStatus? Status { get; set; }
        public string Strengths { get; set; }
        public string Support { get; set; }
        public List<TrainingCoursesViewModel> TrainingCourses { get; set; }
        public List<WorkExperienceViewModel> WorkExperiences { get; set; }

        public bool HasNoQualifications => Qualifications.Any() == false;
        public bool HasNoTrainingCourses => TrainingCourses.Any() == false;
        public bool HasNoWorkExperience => WorkExperiences.Any() == false;
        public bool HasSkills => Skills.Any();
        public bool HasNoSkills => !HasSkills;
        public bool HasNoSupportRequirements => string.IsNullOrWhiteSpace(Support);
        public bool CanNotChangeOutcome => (Status == ApplicationReviewStatus.Successful || Status == ApplicationReviewStatus.Unsuccessful);
        public bool CanChangeOutcome => !CanNotChangeOutcome;
        public bool IsStatusEmployerUnsuccessful => Status == ApplicationReviewStatus.EmployerUnsuccessful;
        public bool IsStatusEmployerInterviewing => Status == ApplicationReviewStatus.EmployerInterviewing;
        public bool ShowDisabilityStatusAlert => DisabilityStatus == ApplicationReviewDisabilityStatus.Yes;

        public ApplicationReviewStatus? Outcome { get; set; }
        public string CandidateFeedback { get; set; }
        public string EmployerFeedback { get; set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(Outcome)
        };

        public long Ukprn { get; set; }
        public Guid? VacancyId { get; set; }
        public Guid ApplicationReviewId { get; set; }
        public string AdditionalQuestionAnswer1 { get; set; }
        public string AdditionalQuestionAnswer2 { get; set; }
        public string AdditionalQuestion1 { get; set; }
        public string AdditionalQuestion2 { get; set; }
        public bool HasAdditionalQuestions => !string.IsNullOrEmpty(AdditionalQuestion1) || !string.IsNullOrEmpty(AdditionalQuestion2);
        public bool HasAdditionalSecondQuestion => !string.IsNullOrEmpty(AdditionalQuestion2);
        public bool CanShowRadioButtonReview => Status == ApplicationReviewStatus.New;
        public bool CanShowRadioButtonShared => (Status == ApplicationReviewStatus.New || Status == ApplicationReviewStatus.InReview);
        public bool CanShowRadioButtonInterviewing => (Status == ApplicationReviewStatus.New || Status == ApplicationReviewStatus.InReview || Status == ApplicationReviewStatus.Shared || Status == ApplicationReviewStatus.EmployerInterviewing);
        public bool NavigateToFeedbackPage { get; set; }
        public string WhatIsYourInterest { get; set; }
        public List<WorkExperienceViewModel> Jobs { get; set; }
        public bool IsFaaV2Application { get; set; }
        public bool HasNoJobs => Jobs.Any() == false;
        
        public List<QualificationTypeDisplay> QualificationTypes
        {
            get
            {
                return
                [
                    new QualificationTypeDisplay { Order = 1, QualificationType = "Degree", HeaderName = "Degree", IsMultiAdd = false, CanShowLevel = false, ShouldDisplayAdditionalInformationField= true },
                    new QualificationTypeDisplay { Order = 1, QualificationType = "Apprenticeship", HeaderName = "Apprenticeship", IsMultiAdd = false, CanShowLevel = false, ShouldDisplayAdditionalInformationField= true},
                    new QualificationTypeDisplay { Order = 1, QualificationType = "A Level", HeaderName = "A Levels", IsMultiAdd = true , CanShowLevel = false, ShouldDisplayAdditionalInformationField= false},
                    new QualificationTypeDisplay { Order = 1, QualificationType = "AS Level", HeaderName = "AS Levels", IsMultiAdd = true  , CanShowLevel = false, ShouldDisplayAdditionalInformationField= false},
                    new QualificationTypeDisplay { Order = 1, QualificationType = "T Level", HeaderName = "T Level", IsMultiAdd = false  , CanShowLevel = false, ShouldDisplayAdditionalInformationField= false},
                    new QualificationTypeDisplay { Order = 1, QualificationType = "BTEC", HeaderName = "BTEC", IsMultiAdd = false  , CanShowLevel = true, ShouldDisplayAdditionalInformationField= false},
                    new QualificationTypeDisplay { Order = 1, QualificationType = "GCSE", HeaderName = "GCSEs", IsMultiAdd = true  , CanShowLevel = false, ShouldDisplayAdditionalInformationField= false},
                    new QualificationTypeDisplay { Order = 1, QualificationType = "Other", HeaderName = "Other", IsMultiAdd = false  , CanShowLevel = false, ShouldDisplayAdditionalInformationField= true}
                ];
            }
        }

        public class QualificationTypeDisplay
        {
            public int Order { get; set; }
            public string QualificationType { get; set; }
            public string HeaderName { get; set; }
            public bool IsMultiAdd { get; set; }
            public bool CanShowLevel { get; set; }
            public bool ShouldDisplayAdditionalInformationField { get; set; }
        }
        public List<QualificationGroup> BuildQualificationGroups()
        {

            var qualificationGroups = new List<QualificationGroup>();
            
            foreach (var qualificationType in QualificationTypes.OrderBy(x => x.Order))
            {
                if (Qualifications.All(x => !x.QualificationType.Equals(qualificationType.QualificationType, StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }

                if (qualificationType.IsMultiAdd)
                {
                    var group = MapGroup(qualificationType, Qualifications.Where(x => x.QualificationType.Equals(qualificationType.QualificationType, StringComparison.CurrentCultureIgnoreCase)));
                    qualificationGroups.Add(group);
                }
                else
                {
                    foreach (var qualification in Qualifications.Where(x => x.QualificationType.Equals(qualificationType.QualificationType, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        var group = MapGroup(qualificationType, new[] { qualification });
                        qualificationGroups.Add(group);
                    }
                }
            }

            return qualificationGroups;
        }
        private static QualificationGroup MapGroup(QualificationTypeDisplay qualificationType,
            IEnumerable<QualificationViewModel> qualifications)
        {
            var result = new QualificationGroup
            {
                DisplayName = qualificationType.HeaderName,
                QualificationType = qualificationType.QualificationType,
                ShowAdditionalInformation = qualificationType.ShouldDisplayAdditionalInformationField,
                AllowMultipleAdd = qualificationType.IsMultiAdd,
                ShowLevel = qualificationType.CanShowLevel,
                Qualifications = qualifications
                    .Select(x => new QualificationViewModel
                    {
                        Subject = x.Subject.Contains('|') ? x.Subject.Split('|')[1] : x.Subject,
                        Grade = x.Grade,
                        AdditionalInformation = x.AdditionalInformation,
                        IsPredicted = x.IsPredicted
                    }).ToList()
            };

            return result;
        }
        public class QualificationGroup
        {
            public string QualificationType { get; set; }
            public string DisplayName { get; set; }
            public List<QualificationViewModel> Qualifications { get; set; } = new List<QualificationViewModel>();
            public bool AllowMultipleAdd { get; set; }
            public bool ShowAdditionalInformation { get; set; }
            public bool? ShowLevel { get; set; }
        }
    }
}
