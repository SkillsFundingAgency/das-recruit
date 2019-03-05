using Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation;
using FluentAssertions;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using System.Threading.Tasks;
using System.Threading;
using System;
using AutoFixture;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    public class CloneVacancyCommandHandlerTests : VacancyValidationTestsBase
    {
        [Fact]
        public async Task CheckClonedVacancyHasCorrectFieldsSet()
        {
            var newVacancyId = Guid.NewGuid();
            var existingVacancy = GetTestVacancy();
            var currentTime = DateTime.UtcNow;
            var startDate = DateTime.Now.AddDays(20);
            var closingDate = DateTime.Now.AddDays(10);
            Vacancy clone = null;

            var mockRepository = new Mock<IVacancyRepository>();
            var mockTimeProvider = new Mock<ITimeProvider>();

            mockTimeProvider.Setup(x => x.Now).Returns(currentTime);
            mockRepository.Setup(x => x.GetVacancyAsync(existingVacancy.Id)).ReturnsAsync(existingVacancy);
            mockRepository.Setup(x => x.CreateAsync(It.IsAny<Vacancy>()))
                            .Callback<Vacancy>(arg => clone = arg)
                            .Returns(Task.CompletedTask);

            var handler = new CloneVacancyCommandHandler(
                Mock.Of<ILogger<CloneVacancyCommandHandler>>(),
                mockRepository.Object,
                Mock.Of<IMessaging>(),
                mockTimeProvider.Object
            );

            var user = new VacancyUser { Name = "Test", Email = "test@test.com", UserId = "123" };
            
            var command = new CloneVacancyCommand(
                cloneVacancyId: existingVacancy.Id, 
                newVacancyId: newVacancyId, 
                user: user, 
                sourceOrigin: SourceOrigin.EmployerWeb,
                startDate: startDate,
                closingDate: closingDate);

            await handler.Handle(command, new CancellationToken());

            AssertUpdatedProperties(existingVacancy, currentTime, clone, command, startDate, closingDate);
            AssertNulledOutProperties(clone);
            AssertUnchangedProperties(existingVacancy, clone);
        }

        private static void AssertUpdatedProperties(Vacancy existingVacancy, DateTime currentTime, Vacancy clone, 
            CloneVacancyCommand command, DateTime startDate, DateTime closingDate)
        {
            // Check properties that should have been updated to new values to the original
            clone.Id.Should().Be(command.NewVacancyId, "{0} should be updated", nameof(clone.Id));
            clone.CreatedByUser.Should().BeEquivalentTo(command.User, "{0} should be updated", nameof(clone.CreatedByUser));
            clone.CreatedDate.Should().Be(currentTime, "{0} should be updated", nameof(clone.CreatedDate));
            clone.LastUpdatedByUser.Should().Be(command.User, "{0} should be updated", nameof(clone.LastUpdatedByUser));
            clone.LastUpdatedDate.Should().Be(currentTime, "{0} should be correct", nameof(clone.LastUpdatedDate));
            clone.SourceOrigin.Should().Be(SourceOrigin.EmployerWeb, "{0} should be updated", nameof(clone.SourceOrigin));
            clone.SourceType.Should().Be(SourceType.Clone, "{0} should be updated", nameof(clone.SourceType));
            clone.SourceVacancyReference.Should().Be(existingVacancy.VacancyReference, "{0} should be updated", nameof(clone.SourceVacancyReference));
            clone.Status.Should().Be(VacancyStatus.Draft, "{0} should be updated", nameof(clone.Status));
            clone.IsDeleted.Should().Be(false, "{0} should be updated", nameof(clone.IsDeleted));
            clone.StartDate.Should().Be(startDate, "{0} should be updated", nameof(clone.StartDate));
            clone.ClosingDate.Should().Be(closingDate, "{0} should be updated", nameof(clone.ClosingDate));
        }

        private static void AssertNulledOutProperties(Vacancy clone)
        {
            // Check roperties that should have been set to null
            clone.VacancyReference.Should().BeNull();
            clone.ApprovedDate.Should().BeNull();
            clone.ClosedDate.Should().BeNull();
            clone.DeletedByUser.Should().BeNull();
            clone.DeletedDate.Should().BeNull();
            clone.LiveDate.Should().BeNull();
            clone.SubmittedByUser.Should().BeNull();
            clone.SubmittedDate.Should().BeNull();
        }

        private static void AssertUnchangedProperties(Vacancy existingVacancy, Vacancy clone)
        {
            // Check all properties that should have the same value as the original
            existingVacancy.Should().BeEquivalentTo(clone, options => options
                                            .Excluding(v => v.Id)
                                            .Excluding(v => v.CreatedByUser)
                                            .Excluding(v => v.CreatedDate)
                                            .Excluding(v => v.LastUpdatedByUser)
                                            .Excluding(v => v.LastUpdatedDate)
                                            .Excluding(v => v.SourceOrigin)
                                            .Excluding(v => v.SourceType)
                                            .Excluding(v => v.SourceVacancyReference)
                                            .Excluding(v => v.Status)
                                            .Excluding(v => v.IsDeleted)
                                            .Excluding(v => v.VacancyReference)
                                            .Excluding(v => v.ApprovedDate)
                                            .Excluding(v => v.ClosedDate)
                                            .Excluding(v => v.DeletedByUser)
                                            .Excluding(v => v.DeletedDate)
                                            .Excluding(v => v.LiveDate)
                                            .Excluding(v => v.SubmittedByUser)
                                            .Excluding(v => v.SubmittedDate)
                                            .Excluding(v => v.CanClose)
                                            .Excluding(v => v.CanDelete)
                                            .Excluding(v => v.CanEdit)
                                            .Excluding(v => v.CanSubmit)
                                            .Excluding(v => v.CanApprove)
                                            .Excluding(v => v.CanRefer)
                                            .Excluding(v => v.CanMakeLive)
                                            .Excluding(v => v.CanSendForReview)
                                            .Excluding(v => v.IsDisabilityConfident)
                                            .Excluding(v => v.CanClone)
                                            .Excluding(v => v.StartDate)
                                            .Excluding(v => v.ClosingDate)
                        );
        }

        private static Vacancy GetTestVacancy()
        {
            var fixture = new Fixture();
            var vacancy = fixture.Create<Vacancy>();

            // Set enum values to be non zero so not assuming a default value.
            vacancy.DisabilityConfident = DisabilityConfident.Yes;
            vacancy.GeoCodeMethod = GeoCodeMethod.PostcodesIo;
            vacancy.SourceOrigin = SourceOrigin.EmployerWeb;
            vacancy.SourceType = SourceType.Extension;
            vacancy.Status = VacancyStatus.Live;
            vacancy.Wage.DurationUnit = DurationUnit.Year;
            vacancy.Wage.WageType = WageType.NationalMinimumWage;

            return vacancy;
        }
    }
}