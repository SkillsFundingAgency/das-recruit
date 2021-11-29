using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Commands;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Recruit.Api.UnitTests.Commands
{
    public class WhenCreatingVacancy
    {
        [Test, MoqAutoData]
        public async Task Then_The_Provider_Is_Looked_Up_And_Error_Returned_If_Not_Exists(
            CreateVacancyCommand command,
            [Frozen]Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen]Mock<ITrainingProviderService> trainingProviderService,
            CreateVacancyCommandHandler handler)
        {
            command.ValidateOnly = false;
            trainingProviderService.Setup(x => x.GetProviderAsync(command.VacancyUserDetails.Ukprn.Value))
                .ReturnsAsync((TrainingProvider) null);
            
            var actual = await handler.Handle(command, CancellationToken.None);
            
            actual.ResultCode.Should().Be(ResponseCode.InvalidRequest);
            actual.ValidationErrors.Should().Contain(error => 
                ((DetailedValidationError)error).Field.Equals(nameof(command.VacancyUserDetails.Ukprn), StringComparison.InvariantCultureIgnoreCase) && 
                ((DetailedValidationError)error).Message.Equals("Training Provider UKPRN not valid"));
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Command_Is_Validated_For_ProviderOwner(
            CreateVacancyCommand command,
            [Frozen]Mock<IRecruitVacancyClient> recruitVacancyClient,
            CreateVacancyCommandHandler handler)
        {
            command.ValidateOnly = false;
            command.VacancyUserDetails.Email = null;
            
            await handler.Handle(command, CancellationToken.None);
            
            recruitVacancyClient.Verify(x=>x.Validate(command.Vacancy, VacancyRuleSet.All), Times.Once);
            recruitVacancyClient.Verify(x=>x.Validate(It.Is<Vacancy>(c=>c.OwnerType == OwnerType.Provider), VacancyRuleSet.All), Times.Once);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Command_Is_Validated_For_EmployerOwner(
            CreateVacancyCommand command,
            [Frozen]Mock<IRecruitVacancyClient> recruitVacancyClient,
            CreateVacancyCommandHandler handler)
        {   
            command.ValidateOnly = false;
            
            await handler.Handle(command, CancellationToken.None);
            
            recruitVacancyClient.Verify(x=>x.Validate(command.Vacancy, VacancyRuleSet.All), Times.Once);
            recruitVacancyClient.Verify(x=>x.Validate(It.Is<Vacancy>(c=>c.OwnerType == OwnerType.Employer), VacancyRuleSet.All), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Not_Valid_Then_Returns_Response_With_Errors(
            EntityValidationError entityValidationError,
            CreateVacancyCommand command,
            [Frozen]Mock<IRecruitVacancyClient> recruitVacancyClient,
            CreateVacancyCommandHandler handler)
        {
            command.ValidateOnly = false;
            var entityValidationResult = new EntityValidationResult{Errors = new List<EntityValidationError>{entityValidationError}};
            recruitVacancyClient.Setup(x => x.Validate(It.IsAny<Vacancy>(), VacancyRuleSet.All))
                .Returns(entityValidationResult);
            
            var actual = await handler.Handle(command, CancellationToken.None);

            actual.ValidationErrors.Should().BeEquivalentTo(
                entityValidationResult.Errors.Select(error => new DetailedValidationError
                {
                    Field = error.PropertyName, 
                    Message = error.ErrorMessage
                }));
            actual.ResultCode.Should().Be(ResponseCode.InvalidRequest);
        }
        
        [Test, MoqAutoData]
        public async Task And_ValidateOnly_True_And_Submitted_By_Provider_Then_Returns_After_Validation(
            CreateVacancyCommand command,
            TrainingProvider provider,
            Vacancy vacancy,
            DateTime timeNow,
            [Frozen]Mock<ITimeProvider> timeProvider,
            [Frozen]Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen]Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen]Mock<IVacancyRepository> vacancyRepository,
            [Frozen]Mock<ITrainingProviderService> trainingProviderService,
            [Frozen]Mock<IProviderRelationshipsService> providerRelationshipsService,
            CreateVacancyCommandHandler handler)
        {
            command.ValidateOnly = true;
            command.VacancyUserDetails.Email = null;
            vacancy.Id = command.Vacancy.Id;
            vacancy.ProgrammeId = command.Vacancy.ProgrammeId;
            trainingProviderService.Setup(x => x.GetProviderAsync(command.VacancyUserDetails.Ukprn.Value))
                .ReturnsAsync(provider);
            recruitVacancyClient.Setup(x => x.Validate(It.IsAny<Vacancy>(), VacancyRuleSet.All))
                .Returns(new EntityValidationResult());

            var actual = await handler.Handle(command, CancellationToken.None);

            actual.ResultCode.Should().Be(ResponseCode.Created);
            actual.Data.Should().Be(1000000001);
            providerVacancyClient.Verify(x => x.CreateProviderApiVacancy(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<VacancyUser>()), 
                Times.Never);
            vacancyRepository.Verify(x=>x.UpdateAsync(It.IsAny<Vacancy>()), 
                Times.Never);
        }
        
        [Test, MoqAutoData]
        public async Task And_ValidateOnly_True_And_Submitted_By_Employer_Then_Returns_After_Validation(
            CreateVacancyCommand command,
            TrainingProvider provider,
            Vacancy vacancy,
            DateTime timeNow,
            [Frozen]Mock<ITimeProvider> timeProvider,
            [Frozen]Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen]Mock<IEmployerVacancyClient> employerVacancyClient,
            [Frozen]Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen]Mock<IVacancyRepository> vacancyRepository,
            [Frozen]Mock<ITrainingProviderService> trainingProviderService,
            [Frozen]Mock<IProviderRelationshipsService> providerRelationshipsService,
            CreateVacancyCommandHandler handler)
        {
            command.ValidateOnly = true;
            vacancy.Id = command.Vacancy.Id;
            vacancy.ProgrammeId = command.Vacancy.ProgrammeId;
            trainingProviderService.Setup(x => x.GetProviderAsync(command.VacancyUserDetails.Ukprn.Value))
                .ReturnsAsync(provider);
            recruitVacancyClient.Setup(x => x.Validate(It.IsAny<Vacancy>(), VacancyRuleSet.All))
                .Returns(new EntityValidationResult());

            var actual = await handler.Handle(command, CancellationToken.None);

            actual.ResultCode.Should().Be(ResponseCode.Created);
            actual.Data.Should().Be(1000000001);
            employerVacancyClient.Verify(x => x.CreateEmployerApiVacancy(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<VacancyUser>(), It.IsAny<TrainingProvider>(), It.IsAny<string>()), 
                Times.Never);
            vacancyRepository.Verify(x=>x.UpdateAsync(It.IsAny<Vacancy>()), 
                Times.Never);
        }

        [Test, MoqAutoData]
        public async Task Then_If_The_Employer_Vacancy_Has_Already_Been_Created_For_The_Given_Id_Then_Error_Returned(
            Exception mongoWriteException,
            CreateVacancyCommand command,
            [Frozen]Mock<IEmployerVacancyClient> recruitVacancyClient,
            [Frozen]Mock<IRecruitVacancyClient> vacancyClient,
            CreateVacancyCommandHandler handler)
        {
            command.ValidateOnly = false;
            vacancyClient.Setup(x => x.Validate(It.IsAny<Vacancy>(), VacancyRuleSet.All))
                .Returns(new EntityValidationResult());
            recruitVacancyClient.Setup(x => x.CreateEmployerApiVacancy(It.IsAny<Guid>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<VacancyUser>(), It.IsAny<TrainingProvider>(), It.IsAny<string>()))
                .ThrowsAsync(mongoWriteException);

            var actual = await handler.Handle(command, CancellationToken.None);
            
            actual.ValidationErrors.Should().Contain(error => 
                ((DetailedValidationError)error).Field.Equals(nameof(command.Vacancy.Id), StringComparison.InvariantCultureIgnoreCase) && 
                ((DetailedValidationError)error).Message.Equals("Unable to create Vacancy. Vacancy already submitted"));
            actual.ResultCode.Should().Be(ResponseCode.InvalidRequest);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_The_Provider_Vacancy_Has_Already_Been_Created_For_The_Given_Id_Then_Error_Returned(
            Exception mongoWriteException,
            CreateVacancyCommand command,
            [Frozen]Mock<IProviderVacancyClient> recruitVacancyClient,
            [Frozen]Mock<IRecruitVacancyClient> vacancyClient,
            CreateVacancyCommandHandler handler)
        {
            command.ValidateOnly = false;
            vacancyClient.Setup(x => x.Validate(It.IsAny<Vacancy>(), VacancyRuleSet.All))
                .Returns(new EntityValidationResult());
            command.VacancyUserDetails.Email = string.Empty;
            recruitVacancyClient.Setup(x => x.CreateProviderApiVacancy(It.IsAny<Guid>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<VacancyUser>()))
                .ThrowsAsync(mongoWriteException);

            var actual = await handler.Handle(command, CancellationToken.None);
            
            actual.ValidationErrors.Should().Contain(error => 
                ((DetailedValidationError)error).Field.Equals(nameof(command.Vacancy.Id), StringComparison.InvariantCultureIgnoreCase) && 
                ((DetailedValidationError)error).Message.Equals("Unable to create Vacancy. Vacancy already submitted"));
            actual.ResultCode.Should().Be(ResponseCode.InvalidRequest);
        }

        [Test, MoqAutoData]
        public async Task Then_If_The_Provider_Has_EmployerReview_Permissions_The_Vacancy_Is_Submitted_For_Review(
            CreateVacancyCommand command,
            TrainingProvider provider,
            Vacancy vacancy,
            DateTime timeNow,
            [Frozen]Mock<ITimeProvider> timeProvider,
            [Frozen]Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen]Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen]Mock<IVacancyRepository> vacancyRepository,
            [Frozen]Mock<ITrainingProviderService> trainingProviderService,
            [Frozen]Mock<IProviderRelationshipsService> providerRelationshipsService,
            CreateVacancyCommandHandler handler)
        {
            command.ValidateOnly = false;
            command.VacancyUserDetails.Email = null;
            vacancy.Id = command.Vacancy.Id;
            vacancy.ProgrammeId = command.Vacancy.ProgrammeId;
            trainingProviderService.Setup(x => x.GetProviderAsync(command.VacancyUserDetails.Ukprn.Value))
                .ReturnsAsync(provider);
            recruitVacancyClient.Setup(x => x.Validate(It.IsAny<Vacancy>(), VacancyRuleSet.All))
                .Returns(new EntityValidationResult());
            recruitVacancyClient.Setup(x => x.GetVacancyAsync(command.Vacancy.Id)).ReturnsAsync(vacancy);
            timeProvider.Setup(x => x.Now).Returns(timeNow);
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(
                    provider.Ukprn.Value, command.Vacancy.EmployerAccountId,
                    command.Vacancy.AccountLegalEntityPublicHashedId, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(true);
            
            var actual = await handler.Handle(command, CancellationToken.None);
            
            actual.ResultCode.Should().Be(ResponseCode.Created);
            actual.Data.Should().Be(vacancy.VacancyReference);
            providerVacancyClient.Verify(x => x.CreateProviderApiVacancy(command.Vacancy.Id, command.Vacancy.Title,
                command.Vacancy.EmployerAccountId, command.VacancyUserDetails), Times.Once);
            vacancyRepository.Verify(x=>x.UpdateAsync(It.Is<Vacancy>(c=>
                c.Id.Equals(command.Vacancy.Id) 
                && c.Status == VacancyStatus.Review
                && c.VacancyReference == vacancy.VacancyReference
                && c.TrainingProvider == provider
            )), Times.Once);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_The_Command_Is_Valid_The_Vacancy_Is_Created_And_Submitted_For_Provider(
            CreateVacancyCommand command,
            TrainingProvider provider,
            Vacancy vacancy,
            DateTime timeNow,
            [Frozen]Mock<ITimeProvider> timeProvider,
            [Frozen]Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen]Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen]Mock<IVacancyRepository> vacancyRepository,
            [Frozen]Mock<ITrainingProviderService> trainingProviderService,
            [Frozen]Mock<IProviderRelationshipsService> providerRelationshipsService,
            CreateVacancyCommandHandler handler)
        {
            command.ValidateOnly = false;
            command.VacancyUserDetails.Email = null;
            vacancy.Id = command.Vacancy.Id;
            vacancy.ProgrammeId = command.Vacancy.ProgrammeId;
            trainingProviderService.Setup(x => x.GetProviderAsync(command.VacancyUserDetails.Ukprn.Value))
                .ReturnsAsync(provider);
            recruitVacancyClient.Setup(x => x.Validate(It.IsAny<Vacancy>(), VacancyRuleSet.All))
                .Returns(new EntityValidationResult());
            recruitVacancyClient.Setup(x => x.GetVacancyAsync(command.Vacancy.Id)).ReturnsAsync(vacancy);
            timeProvider.Setup(x => x.Now).Returns(timeNow);
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(
                    provider.Ukprn.Value, command.Vacancy.EmployerAccountId,
                    command.Vacancy.AccountLegalEntityPublicHashedId, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(false);
            
            var actual = await handler.Handle(command, CancellationToken.None);

            actual.ResultCode.Should().Be(ResponseCode.Created);
            actual.Data.Should().Be(vacancy.VacancyReference);
            providerVacancyClient.Verify(x => x.CreateProviderApiVacancy(command.Vacancy.Id, command.Vacancy.Title,
                command.Vacancy.EmployerAccountId, command.VacancyUserDetails), Times.Once);
            vacancyRepository.Verify(x=>x.UpdateAsync(It.Is<Vacancy>(c=>
                c.Id.Equals(command.Vacancy.Id) 
                && c.Status == VacancyStatus.Submitted
                && c.VacancyReference == vacancy.VacancyReference
                && c.CreatedByUser == vacancy.CreatedByUser
                && c.Title == command.Vacancy.Title
                && c.CreatedByUser == vacancy.CreatedByUser
                && c.CreatedDate == vacancy.CreatedDate
                && c.OwnerType == vacancy.OwnerType
                && c.SourceOrigin == vacancy.SourceOrigin
                && c.SourceType == vacancy.SourceType
                && c.ProgrammeId == command.Vacancy.ProgrammeId
                && c.SubmittedByUser == command.VacancyUserDetails
                && c.LastUpdatedByUser == command.VacancyUserDetails
                && c.SubmittedDate == timeNow
                && c.LastUpdatedDate == timeNow
                && c.TrainingProvider == provider
                )), Times.Once);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_The_Command_Is_Valid_The_Vacancy_Is_Created_And_Submitted_For_Employer(
            CreateVacancyCommand command,
            TrainingProvider provider,
            Vacancy vacancy,
            DateTime timeNow,
            [Frozen]Mock<ITimeProvider> timeProvider,
            [Frozen]Mock<IEmployerVacancyClient> employerVacancyClient,
            [Frozen]Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen]Mock<IVacancyRepository> vacancyRepository,
            [Frozen]Mock<ITrainingProviderService> trainingProviderService,
            CreateVacancyCommandHandler handler)
        {
            command.ValidateOnly = false;
            vacancy.Id = command.Vacancy.Id;
            vacancy.ProgrammeId = command.Vacancy.ProgrammeId;
            trainingProviderService.Setup(x => x.GetProviderAsync(command.VacancyUserDetails.Ukprn.Value))
                .ReturnsAsync(provider);
            recruitVacancyClient.Setup(x => x.Validate(It.IsAny<Vacancy>(), VacancyRuleSet.All))
                .Returns(new EntityValidationResult());
            recruitVacancyClient.Setup(x => x.GetVacancyAsync(command.Vacancy.Id)).ReturnsAsync(vacancy);
            timeProvider.Setup(x => x.Now).Returns(timeNow);
            
            var actual = await handler.Handle(command, CancellationToken.None);

            actual.ResultCode.Should().Be(ResponseCode.Created);
            actual.Data.Should().Be(vacancy.VacancyReference);
            employerVacancyClient.Verify(x => x.CreateEmployerApiVacancy(command.Vacancy.Id, command.Vacancy.Title,
                command.Vacancy.EmployerAccountId, It.Is<VacancyUser>(c => c.Ukprn == null && c.Email.Equals(command.VacancyUserDetails.Email)), 
                provider, command.Vacancy.ProgrammeId), Times.Once);
            vacancyRepository.Verify(x=>x.UpdateAsync(It.Is<Vacancy>(c=>
                c.Id.Equals(command.Vacancy.Id) 
                && c.Status == VacancyStatus.Submitted
                && c.VacancyReference == vacancy.VacancyReference
                && c.CreatedByUser == vacancy.CreatedByUser
                && c.Title == command.Vacancy.Title
                && c.CreatedByUser == vacancy.CreatedByUser
                && c.CreatedDate == vacancy.CreatedDate
                && c.OwnerType == vacancy.OwnerType
                && c.SourceOrigin == vacancy.SourceOrigin
                && c.SourceType == vacancy.SourceType
                && c.ProgrammeId == command.Vacancy.ProgrammeId
                && c.SubmittedByUser == command.VacancyUserDetails
                && c.LastUpdatedByUser == command.VacancyUserDetails
                && c.SubmittedDate == timeNow
                && c.LastUpdatedDate == timeNow
                && c.TrainingProvider == provider
                )), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_If_The_Command_Is_Valid_A_Vacancy_Created_Event_Is_Submitted(
            Vacancy vacancy,
            CreateVacancyCommand command,
            TrainingProvider provider,
            [Frozen]Mock<IEmployerVacancyClient> employerVacancyClient,
            [Frozen]Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen]Mock<IMessaging> messaging,
            [Frozen]Mock<ITrainingProviderService> trainingProviderService,
            CreateVacancyCommandHandler handler)
        {
            command.ValidateOnly = false;
            trainingProviderService.Setup(x => x.GetProviderAsync(command.VacancyUserDetails.Ukprn.Value))
                .ReturnsAsync(provider);
            recruitVacancyClient.Setup(x => x.Validate(It.IsAny<Vacancy>(), VacancyRuleSet.All))
                .Returns(new EntityValidationResult());
            recruitVacancyClient.Setup(x => x.GetVacancyAsync(command.Vacancy.Id)).ReturnsAsync(vacancy);
            
            await handler.Handle(command, CancellationToken.None);
            
            messaging.Verify(x=>x.PublishEvent(It.Is<VacancySubmittedEvent>(c=>
                c.VacancyId.Equals(command.Vacancy.Id)
                && c.VacancyReference.Equals(vacancy.VacancyReference)
                )));
        }

        [Test, MoqAutoData]
        public async Task Then_If_Provider_Owned_And_Employer_Has_To_Review_Then_Review_Event_Is_Submitted(
            Vacancy vacancy,
            CreateVacancyCommand command,
            TrainingProvider provider,
            [Frozen]Mock<IEmployerVacancyClient> employerVacancyClient,
            [Frozen]Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen]Mock<IMessaging> messaging,
            [Frozen]Mock<ITrainingProviderService> trainingProviderService,
            [Frozen]Mock<IProviderRelationshipsService> providerRelationshipsService,
            CreateVacancyCommandHandler handler)
        {
            command.ValidateOnly = false;
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(
                    provider.Ukprn.Value, command.Vacancy.EmployerAccountId,
                    command.Vacancy.AccountLegalEntityPublicHashedId, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(true);
            command.VacancyUserDetails.Email = string.Empty;
            trainingProviderService.Setup(x => x.GetProviderAsync(command.VacancyUserDetails.Ukprn.Value))
                .ReturnsAsync(provider);
            recruitVacancyClient.Setup(x => x.Validate(It.IsAny<Vacancy>(), VacancyRuleSet.All))
                .Returns(new EntityValidationResult());
            recruitVacancyClient.Setup(x => x.GetVacancyAsync(command.Vacancy.Id)).ReturnsAsync(vacancy);
            
            await handler.Handle(command, CancellationToken.None);
            
            messaging.Verify(x=>x.PublishEvent(It.Is<VacancyReviewedEvent>(c=>
                c.VacancyId.Equals(command.Vacancy.Id)
                && c.VacancyReference.Equals(vacancy.VacancyReference)
                && c.Ukprn.Equals(provider.Ukprn)
            )));
        }
    }
}