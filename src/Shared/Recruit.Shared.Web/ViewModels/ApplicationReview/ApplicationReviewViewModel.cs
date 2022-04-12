using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview
{
    public class ApplicationReviewViewModel 
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

        public bool HasNoQualifications => Qualifications.Any() == false;
        public bool HasNoTrainingCourses => TrainingCourses.Any() == false;
        public bool HasNoWorkExperience => WorkExperiences.Any() == false;
        public bool HasSkills => Skills.Any();
        public bool HasNoSkills => !HasSkills;
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
        
        public long Ukprn { get; set; }
        public Guid? VacancyId { get; set; }
        public Guid ApplicationReviewId { get; set; }
    }
}
