using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Controllers;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Recruit.Api.UnitTests.Controllers;

public class ApplicationsControllerTests
{
    [Test, MoqAutoData]
    public async Task Then_The_Request_Is_Handled_And_MediatrEvent_Published(
        long vacancyRef,
        Guid candidateId,
        ApplicationReviewDisabilityStatus disabilityStatus,
        CandidateApplication request,
        [Frozen] Mock<IMessaging> messaging,
        [Greedy]ApplicationsController controller)
    {
        request.VacancyReference = vacancyRef.ToString();
        request.DisabilityConfidenceStatus = disabilityStatus.ToString();
        var actual = await controller.Post(candidateId, request) as CreatedResult;

        actual.Should().NotBeNull();
        messaging.Verify(x => x.PublishEvent(It.Is<ApplicationSubmittedEvent>(c => 
                c.Application.IsFaaV2Application
                && c.Application.VacancyReference == vacancyRef
                && c.Application.Email == request.Email
                && c.Application.Improvements == request.Improvements
                && c.Application.Phone == request.Phone
                && c.Application.Postcode == request.Postcode
                && c.Application.AdditionalQuestion1 == request.AdditionalQuestion1.AnswerText
                && c.Application.AdditionalQuestion2 == request.AdditionalQuestion2.AnswerText
                && c.Application.AddressLine1 == request.AddressLine1
                && c.Application.AddressLine2 == request.AddressLine2
                && c.Application.AddressLine3 == request.AddressLine3
                && c.Application.AddressLine4 == request.AddressLine4
                && c.Application.ApplicationDate == request.ApplicationDate
                && c.Application.FirstName == request.FirstName
                && c.Application.LastName == request.LastName
                && c.Application.CandidateId == candidateId
                && c.Application.ApplicationId == request.ApplicationId
                && c.Application.WhatIsYourInterest == request.WhatIsYourInterest
                && c.Application.Strengths == request.Strengths
                && c.Application.Support == request.Support
                && c.Application.DisabilityStatus == disabilityStatus
                && c.Application.WorkExperiences.FirstOrDefault().Description == request.WorkExperiences.FirstOrDefault().Description
                && c.Application.WorkExperiences.FirstOrDefault().ToDate == request.WorkExperiences.FirstOrDefault().ToDate
                && c.Application.WorkExperiences.FirstOrDefault().JobTitle == request.WorkExperiences.FirstOrDefault().JobTitle
                && c.Application.WorkExperiences.FirstOrDefault().FromDate == request.WorkExperiences.FirstOrDefault().FromDate
                && c.Application.WorkExperiences.FirstOrDefault().Employer == request.WorkExperiences.FirstOrDefault().Employer
                && c.Application.BirthDate == request.DateOfBirth
                )),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Then_The_Withdraw_Request_Is_Handled_And_MediatrEvent_Published(
        Guid candidateId,
        long vacancyRef,
        [Frozen] Mock<IMessaging> messaging,
        [Greedy]ApplicationsController controller)
    {
        var actual = await controller.Withdraw(candidateId, vacancyRef) as CreatedResult;

        actual.Should().NotBeNull();
        messaging.Verify(x => x.PublishEvent(It.Is<ApplicationWithdrawnEvent>(c =>
            c.VacancyReference == vacancyRef
            && c.CandidateId == candidateId)), Times.Once);
    }
}