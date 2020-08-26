using System;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Recruit.Vacancies.Jobs.UnitTests.Triggers.QueueTriggers
{
    public class CommunicationsHouseKeepingQueueTriggerTests
    {
        private const int Days = 180;
        private readonly Mock<ILogger<CommunicationsHouseKeepingQueueTrigger>> _loggerMock = new Mock<ILogger<CommunicationsHouseKeepingQueueTrigger>>();
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig = new RecruitWebJobsSystemConfiguration() { HardDeleteCommunicationMessagesStaleByDays = Days };
        private readonly Mock<ITimeProvider> _timeProviderMock = new Mock<ITimeProvider>();
        private readonly Mock<ICommunicationRepository> _communicationRepositoryMock = new Mock<ICommunicationRepository>();

        [Fact]
        public async Task Delete_Communication_Messages_older_than_180_days()
        {
            //Arrange            
            var datetime = new DateTime(2020, 08, 30).AddDays(-180);
            _communicationRepositoryMock.Setup(x => x.HardDelete(datetime)).Returns(Task.CompletedTask);
            var sut = new CommunicationsHouseKeepingQueueTrigger(_loggerMock.Object, _jobsConfig, _timeProviderMock.Object, _communicationRepositoryMock.Object);
            var message = new CommunicationsHouseKeepingQueueMessage() { CreatedByScheduleDate = new DateTime(2020, 08, 30) };
            
            //Act
            await sut.CommunicationsHouseKeepingAsync(JsonConvert.SerializeObject(message), null);

            //Assert
            _communicationRepositoryMock.Verify(c => c.HardDelete(datetime), Times.AtLeastOnce);

        }
    }
}
