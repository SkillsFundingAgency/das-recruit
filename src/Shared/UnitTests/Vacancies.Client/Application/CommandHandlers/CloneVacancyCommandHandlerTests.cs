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
using System.Collections.Generic;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    [Trait("Category", "Unit")]
    public class CloneVacancyCommandHandlerTests
    {
        private readonly Fixture _autoFixture = new Fixture();

        private enum CloneAssertType
        {
            Cloned,
            IsNull,
            Ignore
        }

        [Theory]
        [InlineData(VacancyStatus.Draft)]
        [InlineData(VacancyStatus.Rejected)]
        [InlineData(VacancyStatus.Referred)]
        [InlineData(VacancyStatus.Approved)]
        public async Task IfInvalidVacancyStatus_ShouldThrowError(VacancyStatus invalidCloneStatus)
        {
            var newVacancyId = Guid.NewGuid();
            var existingVacancy = _autoFixture.Build<Vacancy>().With(c => c.Status, invalidCloneStatus).Create();
            var currentTime = DateTime.UtcNow;
            var startDate = DateTime.Now.AddDays(20);
            var closingDate = DateTime.Now.AddDays(10);

            var user = new VacancyUser { Name = "Test", Email = "test@test.com", UserId = "123" };

            var command = new CloneVacancyCommand(
                cloneVacancyId: existingVacancy.Id,
                newVacancyId: newVacancyId,
                user: user,
                sourceOrigin: SourceOrigin.EmployerWeb,
                startDate: startDate,
                closingDate: closingDate);

            var mockRepository = new Mock<IVacancyRepository>();
            var mockTimeProvider = new Mock<ITimeProvider>();

            mockTimeProvider.Setup(x => x.Now).Returns(currentTime);
            mockRepository.Setup(x => x.GetVacancyAsync(existingVacancy.Id)).ReturnsAsync(existingVacancy);

            var handler = new CloneVacancyCommandHandler(
                Mock.Of<ILogger<CloneVacancyCommandHandler>>(),
                mockRepository.Object,
                Mock.Of<IMessaging>(),
                mockTimeProvider.Object
            );

            await Assert.ThrowsAsync<InvalidStateException>(() => handler.Handle(command, new CancellationToken()));
        }

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

            AssertKnownProperties(existingVacancy, clone);

            AssertUpdatedProperties(existingVacancy, currentTime, clone, command, startDate, closingDate);
        }

        private static void AssertKnownProperties(Vacancy original, Vacancy clone)
        {
            var propertyAssertions = new Dictionary<string, Action<Vacancy, Vacancy, string>>
            {
                {nameof(Vacancy.Id), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Ignore)},
                {nameof(Vacancy.EmployerAccountId), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.VacancyReference), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.Status), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Ignore)},
                {nameof(Vacancy.OwnerType), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.SourceOrigin), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Ignore)},
                {nameof(Vacancy.SourceType), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Ignore)},
                {nameof(Vacancy.SourceVacancyReference), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Ignore)},
                {nameof(Vacancy.ClosedDate), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.ClosedByUser), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.CreatedDate), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Ignore)},
                {nameof(Vacancy.CreatedByUser), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Ignore)},
                {nameof(Vacancy.SubmittedDate), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.SubmittedByUser), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.ReviewDate), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.ReviewCount), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Ignore)},
                {nameof(Vacancy.ReviewByUser), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.ApprovedDate), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.LiveDate), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.LastUpdatedDate), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Ignore)},
                {nameof(Vacancy.LastUpdatedByUser), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Ignore)},
                {nameof(Vacancy.IsDeleted), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Ignore)},
                {nameof(Vacancy.DeletedDate), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.DeletedByUser), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.AnonymousReason), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.ApplicationInstructions), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.ApplicationMethod), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.ApplicationUrl), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.ClosingDate), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Ignore)},
                {nameof(Vacancy.Description), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.DisabilityConfident), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.EmployerContact), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.EmployerDescription), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.EmployerLocation), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.EmployerName), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.EmployerNameOption), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.EmployerReviewFieldIndicators), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.EmployerRejectedReason), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.LegalEntityName), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.EmployerWebsiteUrl), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.GeoCodeMethod), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.NumberOfPositions), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.OutcomeDescription), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.ProviderContact), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.ProviderReviewFieldIndicators), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.ProgrammeId), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.Qualifications), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.ShortDescription), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.Skills), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.StartDate), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Ignore)},
                {nameof(Vacancy.ThingsToConsider), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.Title), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.TrainingDescription), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.TrainingProvider), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.Wage), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.ClosureReason), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.ClosureExplanation), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.TransferInfo), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.IsNull)},
                {nameof(Vacancy.AccountLegalEntityPublicHashedId), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.VacancyType), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)},
                {nameof(Vacancy.RouteId), (o, c, s) => AssertProperty(o, c, s, CloneAssertType.Cloned)}
            };

            foreach (var property in typeof(Vacancy).GetProperties())
            {
                if (property.GetSetMethod() == null)
                    continue;

                if (propertyAssertions.ContainsKey(property.Name) == false)
                {
                    Assert.True(false, $"Unknown clone property '{property.Name}'. Do we want to clone this property?");
                }

                var propertyAssert = propertyAssertions[property.Name];
                propertyAssert(original, clone, property.Name);
            }
        }

        private static void AssertProperty(Vacancy original, Vacancy clone, string propertyName, CloneAssertType assertType)
        {
            var originalValue = typeof(Vacancy).GetProperty(propertyName).GetValue(original);
            var cloneValue = typeof(Vacancy).GetProperty(propertyName).GetValue(clone);

            switch (assertType)
            {
                case CloneAssertType.Cloned:
                    originalValue.Should().BeEquivalentTo(cloneValue, "{0} should be the same", propertyName);
                    break;
                case CloneAssertType.IsNull:
                    cloneValue.Should().BeNull($"{0} should be null", propertyName);
                    break;
                case CloneAssertType.Ignore:
                    break;
            }
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

        private Vacancy GetTestVacancy()
        {
            var vacancy = _autoFixture.Create<Vacancy>();

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