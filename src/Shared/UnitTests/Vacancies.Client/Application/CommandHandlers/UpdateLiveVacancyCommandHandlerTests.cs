﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateLiveVacancyCommandHandlerTests
    {
        private Mock<ILogger<UpdateLiveVacancyCommandHandler>> _mockLogger;
        private Mock<IVacancyRepository> _mockRepository;
        private Mock<IMessaging> _mockMessaging;
        private Mock<ITimeProvider> _mockTimeProvider;

        private UpdateLiveVacancyCommandHandler _handler;

        private DateTime _currentTime;
        private Guid _vacancyId = Guid.NewGuid();
        private Vacancy _originalVacancy;
        private Vacancy _updatedVacancy;
        private UpdateLiveVacancyCommand _message;

        [Fact]
        public async Task ShouldPublishLiveVacancyClosingDateChangedEventWhenLiveVacancyClosingDateChanges()
        {
            var newClosingDate = DateTime.UtcNow.AddDays(100);
            _originalVacancy.Status = VacancyStatus.Live;
            _updatedVacancy.ClosingDate = newClosingDate;

            await _handler.Handle(_message, CancellationToken.None);

            _mockMessaging
                .Verify(x => x.PublishEvent(
                    It.Is<LiveVacancyClosingDateChangedEvent>(p =>
                        p.NewClosingDate == newClosingDate
                        && p.VacancyId == _vacancyId
                        && p.VacancyReference == _updatedVacancy.VacancyReference
                    )));
        }

        [Fact]
        public async Task ShouldNotPublishLiveVacancyClosingDateChangedEventWhenLiveVacancyClosingDateIsUnchanged()
        {
            var newClosingDate = _originalVacancy.ClosingDate;
            _originalVacancy.Status = VacancyStatus.Live;
            _updatedVacancy.ClosingDate = newClosingDate;

            await _handler.Handle(_message, CancellationToken.None);

            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<LiveVacancyClosingDateChangedEvent>()), Times.Never);
        }

        [Fact]
        public async Task ShouldNotPublishLiveVacancyClosingDateChangedEventWhenVacancyIsNotLive()
        {
            var nonLiveStatuses = Enum.GetValues(typeof(VacancyStatus))
                .OfType<VacancyStatus>()
                .Where(x => x != VacancyStatus.Live);

            foreach (VacancyStatus currentStatus in nonLiveStatuses)
            {
                var newClosingDate = DateTime.UtcNow.AddDays(100);
                _originalVacancy.Status = currentStatus;
                _updatedVacancy.ClosingDate = newClosingDate;

                await _handler.Handle(_message, CancellationToken.None);

                _mockMessaging.Verify(x =>
                    x.PublishEvent(It.IsAny<LiveVacancyClosingDateChangedEvent>()),
                    Times.Never,
                    $"Should not publish message when Vacancy.Status is {currentStatus}");
            }
        }

        public UpdateLiveVacancyCommandHandlerTests()
        {
            _currentTime = DateTime.UtcNow;

            _originalVacancy = CreateVacancy();
            _updatedVacancy = CreateVacancy();

            _message = new UpdateLiveVacancyCommand
            {
                Vacancy = _updatedVacancy
            };

            _mockLogger = new Mock<ILogger<UpdateLiveVacancyCommandHandler>>();
            _mockMessaging = new Mock<IMessaging>();

            _mockRepository = new Mock<IVacancyRepository>();
            _mockRepository
                .Setup(x => x.GetVacancyAsync(_vacancyId))
                .ReturnsAsync(_originalVacancy);

            _mockTimeProvider = new Mock<ITimeProvider>();
            _mockTimeProvider
                .Setup(x => x.Now)
                .Returns(() => _currentTime);

            _handler = new UpdateLiveVacancyCommandHandler(
                _mockLogger.Object,
                _mockRepository.Object,
                _mockMessaging.Object,
                _mockTimeProvider.Object);
        }

        private Vacancy CreateVacancy()
        {
            return new Vacancy
            {
                Id = _vacancyId,
                VacancyReference = 299792458,
                ClosingDate = DateTime.UtcNow
            };
        }
    }
}
