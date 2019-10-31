using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Esfa.Recruit.Vacancies.Jobs.UnitTests.Triggers.QueueTriggers
{
    public class DeleteStaleVacanciesQueueTriggerTests
    {
        private readonly Mock<ILogger<DeleteStaleVacanciesQueueTrigger>> _mockLogger = new Mock<ILogger<DeleteStaleVacanciesQueueTrigger>>(); 
        private readonly RecruitWebJobsSystemConfiguration _config = new RecruitWebJobsSystemConfiguration(); 
        private readonly Mock<ITimeProvider> _mockTimeProvider = new Mock<ITimeProvider>();
        private readonly Mock<IVacancyQuery> _mockQuery = new Mock<IVacancyQuery>();
        private readonly Mock<IMessaging> _mockMessaging = new Mock<IMessaging>();
        private readonly DeleteStaleVacanciesQueueMessage _message = new DeleteStaleVacanciesQueueMessage();
        private string GetMessage() => JsonConvert.SerializeObject(_message);
        private DeleteStaleVacanciesQueueTrigger GetSut() => new DeleteStaleVacanciesQueueTrigger(_mockLogger.Object, _config, _mockTimeProvider.Object, _mockQuery.Object, _mockMessaging.Object);
        
        [Fact]
        public async Task WhenConfigIsNotPopulated_ThenUseDefaultStaleByDays()
        {
            var executionDate = new DateTime(2019, 10, 8);
            _mockTimeProvider.Setup(t => t.Today).Returns(executionDate);
            var expectedDraftStaleByDate = executionDate.AddDays(DeleteStaleVacanciesQueueTrigger.DefaultDraftStaleByDays * -1);
            var expectedReferredStaleByDate = executionDate.AddDays(DeleteStaleVacanciesQueueTrigger.DefaultReferredStaleByDays * -1);

            var sut = GetSut();
            await sut.DeleteStaleVacanciesAsync(GetMessage(), null);

            _mockQuery.Verify(q => q.GetDraftVacanciesCreatedBeforeAsync<VacancyIdentifier>(expectedDraftStaleByDate));
            _mockQuery.Verify(q => q.GetReferredVacanciesSubmittedBeforeAsync<VacancyIdentifier>(expectedReferredStaleByDate));
        }

        [Fact]
        public async Task WhenStaleByDaysAreDefinedInTheConfig_ThenUseConfigStaleByDays()
        {
            _config.DraftVacanciesStaleByDays = 20;
            _config.ReferredVacanciesStaleByDays = 10;
            var executionDate = new DateTime(2019, 10, 8);
            _mockTimeProvider.Setup(t => t.Today).Returns(executionDate);
            var expectedDraftStaleByDate = executionDate.AddDays(_config.DraftVacanciesStaleByDays.Value * -1);
            var expectedReferredStaleByDate = executionDate.AddDays(_config.ReferredVacanciesStaleByDays.Value * -1);

            var sut = GetSut();
            await sut.DeleteStaleVacanciesAsync(GetMessage(), null);

            _mockQuery.Verify(q => q.GetDraftVacanciesCreatedBeforeAsync<VacancyIdentifier>(expectedDraftStaleByDate));
            _mockQuery.Verify(q => q.GetReferredVacanciesSubmittedBeforeAsync<VacancyIdentifier>(expectedReferredStaleByDate));
        }

        [Fact]
        public async Task WhenDateIsMissingInTheMessage_ThenUseTimeProviderDate()
        {
            _message.CreatedByScheduleDate = null;
            var executionDate = new DateTime(2019, 10, 8);
            _mockTimeProvider.Setup(t => t.Today).Returns(executionDate);

            var expectedDraftStaleByDate = executionDate.AddDays(DeleteStaleVacanciesQueueTrigger.DefaultDraftStaleByDays * -1);
            var expectedReferredStaleByDate = executionDate.AddDays(DeleteStaleVacanciesQueueTrigger.DefaultReferredStaleByDays * -1);

            var sut = GetSut();
            await sut.DeleteStaleVacanciesAsync(GetMessage(), null);

            _mockTimeProvider.Verify(t => t.Today);
            _mockQuery.Verify(q => q.GetDraftVacanciesCreatedBeforeAsync<VacancyIdentifier>(expectedDraftStaleByDate));
            _mockQuery.Verify(q => q.GetReferredVacanciesSubmittedBeforeAsync<VacancyIdentifier>(expectedReferredStaleByDate));
        }
        
        [Fact]
        public async Task WhenDateIsDefinedInTheMessage_ThenUseMessageDate()
        {
            var executionDate = new DateTime(2019, 10, 8);
            _message.CreatedByScheduleDate = executionDate;

            var expectedDraftStaleByDate = executionDate.AddDays(DeleteStaleVacanciesQueueTrigger.DefaultDraftStaleByDays * -1);
            var expectedReferredStaleByDate = executionDate.AddDays(DeleteStaleVacanciesQueueTrigger.DefaultReferredStaleByDays * -1);

            var sut = GetSut();
            await sut.DeleteStaleVacanciesAsync(GetMessage(), null);

            _mockTimeProvider.Verify(t => t.Today, Times.Never);
            _mockQuery.Verify(q => q.GetDraftVacanciesCreatedBeforeAsync<VacancyIdentifier>(expectedDraftStaleByDate));
            _mockQuery.Verify(q => q.GetReferredVacanciesSubmittedBeforeAsync<VacancyIdentifier>(expectedReferredStaleByDate));
        }

        [Fact]
        public async Task WhenDraftAndReferredVacanciesFound_RaiseDeleteCommandForAll()
        {
            var executionDate = new DateTime(2019, 10, 8);
            _message.CreatedByScheduleDate = executionDate;
            var fixture = new Fixture();
            var draftVacancy = fixture.Create<VacancyIdentifier>();
            var referredVacancy = fixture.Create<VacancyIdentifier>();
            _mockQuery.Setup(q => q.GetDraftVacanciesCreatedBeforeAsync<VacancyIdentifier>(It.IsAny<DateTime>())).ReturnsAsync(new [] { draftVacancy });
            _mockQuery.Setup(q => q.GetReferredVacanciesSubmittedBeforeAsync<VacancyIdentifier>(It.IsAny<DateTime>())).ReturnsAsync(new [] { referredVacancy });

            var sut = GetSut();
            await sut.DeleteStaleVacanciesAsync(GetMessage(), null);

            _mockMessaging.Verify(m => m.SendCommandAsync(It.IsAny<ICommand>()), Times.Exactly(2));
            _mockMessaging.Verify(m => m.SendCommandAsync(It.Is<DeleteVacancyCommand>(d => d.VacancyId == draftVacancy.Id)));
            _mockMessaging.Verify(m => m.SendCommandAsync(It.Is<DeleteVacancyCommand>(d => d.VacancyId == referredVacancy.Id)));
        }

        [Fact]
        public async Task WhenReferredVacanciesFound_RaiseDeleteCommand()
        {
            var executionDate = new DateTime(2019, 10, 8);
            _message.CreatedByScheduleDate = executionDate;
            var fixture = new Fixture();
            var vacancies = new [] { fixture.Create<VacancyIdentifier>() };
            _mockQuery
                .Setup(q => q.GetDraftVacanciesCreatedBeforeAsync<VacancyIdentifier>(It.IsAny<DateTime>()))
                .ReturnsAsync(Array.Empty<VacancyIdentifier>());
            _mockQuery
                .Setup(q => q.GetReferredVacanciesSubmittedBeforeAsync<VacancyIdentifier>(It.IsAny<DateTime>()))
                .ReturnsAsync(vacancies);

            var sut = GetSut();
            await sut.DeleteStaleVacanciesAsync(GetMessage(), null);

            _mockMessaging.Verify(m => m.SendCommandAsync(It.IsAny<ICommand>()), Times.Exactly(1));
            _mockMessaging.Verify(m => m.SendCommandAsync(It.Is<DeleteVacancyCommand>(d => d.VacancyId == vacancies.First().Id)));
        }

        [Fact]
        public async Task WhenDraftVacanciesFound_RaiseDeleteCommand()
        {
            var executionDate = new DateTime(2019, 10, 8);
            _message.CreatedByScheduleDate = executionDate;
            var fixture = new Fixture();
            var vacancies = new [] { fixture.Create<VacancyIdentifier>() };
            _mockQuery
                .Setup(q => q.GetDraftVacanciesCreatedBeforeAsync<VacancyIdentifier>(It.IsAny<DateTime>()))
                .ReturnsAsync(vacancies);
            _mockQuery
                .Setup(q => q.GetReferredVacanciesSubmittedBeforeAsync<VacancyIdentifier>(It.IsAny<DateTime>()))
                .ReturnsAsync(Array.Empty<VacancyIdentifier>());

            var sut = GetSut();
            await sut.DeleteStaleVacanciesAsync(GetMessage(), null);

            _mockMessaging.Verify(m => m.SendCommandAsync(It.IsAny<ICommand>()), Times.Exactly(1));
            _mockMessaging.Verify(m => m.SendCommandAsync(It.Is<DeleteVacancyCommand>(d => d.VacancyId == vacancies.First().Id)));
        }

        [Fact]
        public async Task WhenNoVacanciesFound_RaiseDeleteCommand()
        {
            var executionDate = new DateTime(2019, 10, 8);
            _message.CreatedByScheduleDate = executionDate;
            _mockQuery
                .Setup(q => q.GetDraftVacanciesCreatedBeforeAsync<VacancyIdentifier>(It.IsAny<DateTime>()))
                .ReturnsAsync(Array.Empty<VacancyIdentifier>());
            _mockQuery
                .Setup(q => q.GetReferredVacanciesSubmittedBeforeAsync<VacancyIdentifier>(It.IsAny<DateTime>()))
                .ReturnsAsync(Array.Empty<VacancyIdentifier>());

            var sut = GetSut();
            await sut.DeleteStaleVacanciesAsync(GetMessage(), null);

            _mockMessaging.Verify(m => m.SendCommandAsync(It.IsAny<DeleteVacancyCommand>()), Times.Never);
        }
    }
}