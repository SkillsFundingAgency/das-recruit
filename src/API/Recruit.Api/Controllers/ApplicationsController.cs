using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;
using ApplicationQualification = Esfa.Recruit.Vacancies.Client.Domain.Entities.ApplicationQualification;

namespace SFA.DAS.Recruit.Api.Controllers;

[Route("api/[controller]")]
public class ApplicationsController(IMessaging messaging, IMediator mediator) : ApiControllerBase
{
    // GET api/applications/{candidateId}
    [HttpPost]
    [Route("{candidateId}")]
    public async Task<IActionResult> Post([FromRoute]Guid candidateId, [FromBody]CandidateApplication candidateApplication)
    {
        Enum.TryParse<ApplicationReviewDisabilityStatus>(candidateApplication.DisabilityConfidenceStatus,
            out var disabilityConfidentStatus);
        
        var application = new Application
        {
            Email = candidateApplication.Email,
            Improvements = candidateApplication.Improvements,
            Phone = candidateApplication.Phone,
            Postcode = candidateApplication.Postcode,
            AdditionalQuestion1Text = candidateApplication.AdditionalQuestion1?.QuestionText,
            AdditionalQuestion1 = candidateApplication.AdditionalQuestion1?.AnswerText,
            AdditionalQuestion2Text = candidateApplication.AdditionalQuestion2?.QuestionText,
            AdditionalQuestion2 = candidateApplication.AdditionalQuestion2?.AnswerText,
            AddressLine1 = candidateApplication.AddressLine1,
            AddressLine2 = candidateApplication.AddressLine2,
            AddressLine3 = candidateApplication.AddressLine3,
            AddressLine4 = candidateApplication.AddressLine4,
            ApplicationDate = candidateApplication.ApplicationDate,
            FirstName = candidateApplication.FirstName,
            LastName = candidateApplication.LastName,
            CandidateId = candidateId,
            ApplicationId = candidateApplication.ApplicationId,
            Strengths = candidateApplication.Strengths,
            WhatIsYourInterest = candidateApplication.WhatIsYourInterest,
            WorkExperiences = candidateApplication.WorkExperiences.Select(c=> new ApplicationWorkExperience
            {
                Description = c.Description,
                Employer = c.Employer,
                JobTitle = c.JobTitle,
                FromDate = c.FromDate,
                ToDate = c.ToDate ?? DateTime.MinValue
            }).ToList(),
            
            Jobs = candidateApplication.Jobs.Select(c=> new ApplicationJob
            {
                Description = c.Description,
                Employer = c.Employer,
                JobTitle = c.JobTitle,
                FromDate = c.FromDate,
                ToDate = c.ToDate ?? DateTime.MinValue
            }).ToList(),
            
            Qualifications = candidateApplication.Qualifications.Select(c=> new ApplicationQualification
            {
                Grade = c.Grade,
                Subject = c.Subject,
                Year = 0,
                IsPredicted = c.IsPredicted ?? false,
                QualificationType = c.QualificationType,
                AdditionalInformation = c.AdditionalInformation
            }).ToList(),
            
            VacancyReference = Convert.ToInt64(candidateApplication.VacancyReference),
            TrainingCourses = candidateApplication.TrainingCourses.Select(c=> new ApplicationTrainingCourse
            {
                Title = c.Title,
                ToDate = c.ToDate
            }).ToList(),
            
            Support = candidateApplication.Support,
            DisabilityStatus = disabilityConfidentStatus,
            IsFaaV2Application = true,
            BirthDate = candidateApplication.DateOfBirth
        };
        
        await messaging.PublishEvent(new ApplicationSubmittedEvent
        {
            Application = application,
            
        });
        
        return Created();
    }

    [HttpPost]
    [Route("{candidateId}/withdraw/{vacancyRef}")]
    public async Task<IActionResult> Withdraw([FromRoute] Guid candidateId, [FromRoute] long vacancyRef)
    {
        await messaging.PublishEvent(new ApplicationWithdrawnEvent
        {
            CandidateId = candidateId,
            VacancyReference = vacancyRef
        });
        return Created();
    }
    
    [HttpGet("{candidateId:Guid}/application/{applicationReviewId:guid}")]
    public async Task<IActionResult> GetApplicationReview([FromRoute] Guid candidateId, [FromRoute] Guid applicationReviewId)
    {
        var response = await mediator.Send(new GetApplicationReviewQuery(candidateId, applicationReviewId));
        return GetApiResponse(response);
    }
}