using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Application
    {
        public Guid CandidateId { get; set; }
        public long VacancyReference { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime BirthDate { get; set; }
        public ApplicationReviewDisabilityStatus? DisabilityStatus { get; set; }
        public int EducationFromYear { get; set; }
        public string EducationInstitution { get; set; }
        public int EducationToYear { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string HobbiesAndInterests { get; set; }
        public string Improvements { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Postcode { get; set; }
        public List<ApplicationQualification> Qualifications { get; set; }
        public List<string> Skills { get; set; }
        public string Strengths { get; set; }
        public string Support { get; set; }
        public List<ApplicationTrainingCourse> TrainingCourses { get; set; }
        public List<ApplicationWorkExperience> WorkExperiences { get; set; }
        public List<ApplicationJob> Jobs { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public string AdditionalQuestion1 { get; set; }
        public string AdditionalQuestion2 { get; set; }
        public bool IsFaaV2Application { get; set; }
        public string WhatIsYourInterest { get; set; }
        public string AdditionalQuestion1Text { get; set; }
        public string AdditionalQuestion2Text { get; set; }
        public Guid ApplicationId { get; set; }
    }
}
