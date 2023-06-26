using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;

    public class ApplicationReviewViewModel : ApplicationReviewRouteModel
    {
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
        public List<string> Skills { get; set; }
        public ApplicationReviewStatus? Status { get; set; }
        public string Strengths { get; set; }
        public string Support { get; set; }
        public List<TrainingCoursesViewModel> TrainingCourses { get; set; }
        public List<WorkExperienceViewModel> WorkExperiences { get; set; }
        public string AdditionalQuestion1 { get; set; }
        public string AdditionalAnswer1 { get; set; }
        public string AdditionalQuestion2 { get; set; }
        public string AdditionalAnswer2 { get; set; }

        public bool HasNoQualifications => Qualifications.Any() == false;
        public bool HasNoTrainingCourses => TrainingCourses.Any() == false;
        public bool HasNoWorkExperience => WorkExperiences.Any() == false;
        public bool HasSkills => Skills.Any();
        public bool HasNoSkills => !HasSkills;

        public bool HasAdditionalQuestions => !AdditionalQuestion1.IsNullOrEmpty() || !AdditionalQuestion2.IsNullOrEmpty();
        public bool HasAdditionalSecondQuestion => !AdditionalQuestion2.IsNullOrEmpty();
        public bool HasNoSupportRequirements => string.IsNullOrWhiteSpace(Support);
        public bool CanNotChangeOutcome => (Status == ApplicationReviewStatus.Successful || Status == ApplicationReviewStatus.Unsuccessful);
        public bool CanChangeOutcome => !CanNotChangeOutcome;
        public bool ShowDisabilityStatusAlert => DisabilityStatus == ApplicationReviewDisabilityStatus.Yes;

        public ApplicationReviewStatus? Outcome { get; set; }
        public string CandidateFeedback { get; set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(Outcome)
        };
        public bool ShowAnonymisedApplicantDetails => (Status == ApplicationReviewStatus.Shared || Status == ApplicationReviewStatus.Unsuccessful);
        public bool HideRadioButtons => (Status == ApplicationReviewStatus.Interviewing || Status == ApplicationReviewStatus.Unsuccessful);
        public bool IsApplicationUnsuccessful => Status == ApplicationReviewStatus.Unsuccessful;
        public bool IsApplicationShared => Status == ApplicationReviewStatus.Shared;
        public bool IsApplicationInterviewing => Status == ApplicationReviewStatus.Interviewing;
        public string FormHeaderText => (Status == ApplicationReviewStatus.Shared) ? "Do you want to interview this applicant?" : "Outcome";
        public string FormRadioButtonNoText => (Status == ApplicationReviewStatus.Shared) ? "No" : "Unsuccessful";
        public string FormRadioButtonNoFeedbackText => (Status == ApplicationReviewStatus.Shared) ? "Explain why you don't want to interview this applicant. Your comments will be sent to your training provider, who will then give feedback to the applicant."
        : "Explain why the application has been unsuccessful, your comments will be sent to the candidate.";
}