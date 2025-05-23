using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using ApplicationReviewViewModel = Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview.ApplicationReviewViewModel;

namespace Esfa.Recruit.Provider.Web.Mappings.Extensions
{
    public static class ApplicationReviewMapperExtensions
    {
        public static ApplicationReviewViewModel ToViewModel(this ApplicationReview r)
        {
            return new ApplicationReviewViewModel
            {
                Email = r.Application.Email,
                Name = r.Application.FullName,
                TrainingCourses = r.Application.TrainingCourses?.Select(c => new TrainingCoursesViewModel
                {
                    Title = c.Title,
                    FromDate = c.FromDate,
                    Provider = c.Provider,
                    ToDate = c.ToDate
                }).ToList() ?? new List<TrainingCoursesViewModel>(),
                Postcode = r.Application.Postcode,
                AddressLine1 = r.Application.AddressLine1,
                AddressLine2 = r.Application.AddressLine2,
                AddressLine3 = r.Application.AddressLine3,
                AddressLine4 = r.Application.AddressLine4,
                CandidateFeedback = r.CandidateFeedback,
                EmployerFeedback = r.EmployerFeedback,
                DisabilityStatus = r.Application.DisabilityStatus ?? ApplicationReviewDisabilityStatus.Unknown,
                EducationFromYear = r.Application.EducationFromYear,
                EducationInstitution = r.Application.EducationInstitution,
                EducationToYear = r.Application.EducationToYear,
                FriendlyId = r.GetFriendlyId(),
                HobbiesAndInterests = r.Application.HobbiesAndInterests,
                Improvements = r.Application.Improvements,
                WhatIsYourInterest = r.Application.WhatIsYourInterest,
                Phone = r.Application.Phone,
                AdditionalQuestionAnswer1 = r.Application.AdditionalQuestion1,
                AdditionalQuestionAnswer2 = r.Application.AdditionalQuestion2,
                AdditionalQuestion1 = r.Application.IsFaaV2Application ? r.Application.AdditionalQuestion1Text : r.AdditionalQuestion1,
                AdditionalQuestion2 = r.Application.IsFaaV2Application ? r.Application.AdditionalQuestion2Text : r.AdditionalQuestion2,
                Qualifications = r.Application.Qualifications?.Select(q =>
                    new QualificationViewModel
                    {
                        Grade = q.Grade,
                        IsPredicted = q.IsPredicted,
                        QualificationType = q.QualificationType,
                        Subject = q.Subject,
                        Year = q.Year,
                        AdditionalInformation = q.AdditionalInformation,
                        QualificationOrder = q.QualificationOrder
                    }).OrderBy(ord => ord.QualificationOrder).ToList() ?? new List<QualificationViewModel>(),
                Skills = r.Application.Skills ?? new List<string>(),
                Status = r.Status,
                Strengths = r.Application.Strengths,
                Support = r.Application.Support,
                WorkExperiences = r.Application.WorkExperiences?.Select(w => new WorkExperienceViewModel
                {
                    FromDate = w.FromDate,
                    ToDate = w.ToDate,
                    Employer = w.Employer,
                    Description = w.Description,
                    JobTitle = w.JobTitle
                }).ToList() ?? new List<WorkExperienceViewModel>(),
                Jobs = r.Application.Jobs?.Select(w => new WorkExperienceViewModel
                {
                    FromDate = w.FromDate,
                    ToDate = w.ToDate,
                    Employer = w.Employer,
                    Description = w.Description,
                    JobTitle = w.JobTitle
                }).ToList() ?? new List<WorkExperienceViewModel>(),
                IsFaaV2Application = r.Application.IsFaaV2Application
            };
        }
    }
}

