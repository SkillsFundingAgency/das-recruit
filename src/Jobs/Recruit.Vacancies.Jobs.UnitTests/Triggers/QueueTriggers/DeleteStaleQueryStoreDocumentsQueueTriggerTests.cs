using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Esfa.Recruit.Vacancies.Jobs.UnitTests.Triggers.QueueTriggers
{
    public class DeleteStaleQueryStoreDocumentsQueueTriggerTests
    {
        private const int Days = 90;
        private readonly Mock<ILogger<DeleteStaleQueryStoreDocumentsQueueTrigger>> _loggerMock = new Mock<ILogger<DeleteStaleQueryStoreDocumentsQueueTrigger>>();
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig = new RecruitWebJobsSystemConfiguration() { QueryStoreDocumentsStaleAfterDays = Days };
        private readonly Mock<ITimeProvider> _timeProviderMock = new Mock<ITimeProvider>();
        private readonly Mock<IQueryStoreHouseKeepingService> _queryStoreHouseKeepingServiceMock = new Mock<IQueryStoreHouseKeepingService>();

        [Fact]
        public async Task GivenDateInTheMessage_ThenUseThatDate()
        {
            _queryStoreHouseKeepingServiceMock.Setup(s => s.GetStaleDocumentsAsync<QueryProjectionBase>(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(new List<QueryProjectionBase>());
            _queryStoreHouseKeepingServiceMock.Setup(s => s.DeleteStaleDocumentsAsync<QueryProjectionBase>(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(0);
            var sut = new DeleteStaleQueryStoreDocumentsQueueTrigger(_loggerMock.Object, _jobsConfig, _timeProviderMock.Object, _queryStoreHouseKeepingServiceMock.Object);
            var message = new DeleteStaleQueryStoreDocumentsQueueMessage() { CreatedByScheduleDate = DateTime.Today };
            await sut.DeleteStaleQueryStoreDocumentsAsync(JsonConvert.SerializeObject(message), null);
            _queryStoreHouseKeepingServiceMock.Verify(s => s.GetStaleDocumentsAsync<QueryProjectionBase>(It.IsAny<string>(), DateTime.Today.AddDays(Days * -1)), Times.Exactly(5));
            _timeProviderMock.Verify(t => t.Today, Times.Never);
        }

        [Fact]
        public async Task GivenNoDateInTheMessage_ThenUseTodaysDate()
        {
            var targetDate = DateTime.Today.AddDays(-1);
            _timeProviderMock.Setup(t => t.Today).Returns(targetDate);
            _queryStoreHouseKeepingServiceMock.Setup(s => s.GetStaleDocumentsAsync<QueryProjectionBase>(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(new List<QueryProjectionBase>());
            _queryStoreHouseKeepingServiceMock.Setup(s => s.DeleteStaleDocumentsAsync<QueryProjectionBase>(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(0);
            var sut = new DeleteStaleQueryStoreDocumentsQueueTrigger(_loggerMock.Object, _jobsConfig, _timeProviderMock.Object, _queryStoreHouseKeepingServiceMock.Object);
            await sut.DeleteStaleQueryStoreDocumentsAsync("{}", null);
            _queryStoreHouseKeepingServiceMock.Verify(s => s.GetStaleDocumentsAsync<QueryProjectionBase>(It.IsAny<string>(), targetDate.AddDays(Days * -1)), Times.Exactly(5));
            _timeProviderMock.VerifyGet(t => t.Today);
        }

        [Fact]
        public async Task GivenStaleDocumentsExist_ThenDeleteStaleDocuments()
        {
            var viewType = "ClosedVacancy";
            var testId = Guid.NewGuid().ToString();
            _timeProviderMock.Setup(t => t.Today).Returns(DateTime.Today);
            _queryStoreHouseKeepingServiceMock.Setup(s => s.GetStaleDocumentsAsync<QueryProjectionBase>(viewType, It.IsAny<DateTime>())).ReturnsAsync(new List<QueryProjectionBase>() { new ClosedVacancy(){ Id = testId}} );
            _queryStoreHouseKeepingServiceMock.Setup(s => s.GetStaleDocumentsAsync<QueryProjectionBase>(It.IsNotIn(new [] {viewType}), It.IsAny<DateTime>())).ReturnsAsync(new List<QueryProjectionBase>());
            _queryStoreHouseKeepingServiceMock.Setup(s => s.DeleteStaleDocumentsAsync<QueryProjectionBase>(viewType, It.IsAny<IEnumerable<string>>())).ReturnsAsync(1);
            var sut = new DeleteStaleQueryStoreDocumentsQueueTrigger(_loggerMock.Object, _jobsConfig, _timeProviderMock.Object, _queryStoreHouseKeepingServiceMock.Object);
            await sut.DeleteStaleQueryStoreDocumentsAsync("", null);
            _queryStoreHouseKeepingServiceMock.Verify(s => s.GetStaleDocumentsAsync<QueryProjectionBase>(It.IsAny<string>(), DateTime.Today.AddDays(Days * -1)), Times.Exactly(5));
            _queryStoreHouseKeepingServiceMock.Verify(s => s.DeleteStaleDocumentsAsync<QueryProjectionBase>(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()));
        }


    }
}