using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Jobs.Jobs;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Jobs.UnitTests.Jobs
{
    public class TransferVacancyToLegalEntityJobTests
    {
        private const string UserEmail = "test@test.com";
        private const string UserName = "John Smith";
        private readonly Mock<IVacancyRepository> _mockVacancyRepository;
        private readonly Mock<IMessaging> _mockMessaging;
        private readonly TransferVacancyToLegalEntityJob _sut;

        public TransferVacancyToLegalEntityJobTests()
        {
            _mockVacancyRepository = new Mock<IVacancyRepository>();
            _mockMessaging = new Mock<IMessaging>();
            _sut = new TransferVacancyToLegalEntityJob(_mockVacancyRepository.Object, _mockMessaging.Object);
        }

        [Fact]
        public async Task GivenExistingVacancyTransferRequest_ThenShouldProcessVacancyToTransfer()
        {
            var userRef = Guid.NewGuid();
            var vacancyId = Guid.NewGuid();
            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(vacancyId))
                                .ReturnsAsync(new Vacancy { Id = vacancyId });

            await _sut.Run(vacancyId, userRef, UserEmail, UserName, TransferReason.EmployerRevokedPermission);

            _mockMessaging.Verify(x => x.SendCommandAsync(It.Is<TransferVacancyToLegalEntityCommand>(c=>c.Id == vacancyId)), Times.Once);
        }

        [Fact]
        public async Task GivenNonExistingVacancyTransferRequest_ThenShouldNotProcessVacancyToTransfer()
        {
            var userRef = Guid.NewGuid();
            var vacancyId = Guid.NewGuid();
            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(It.IsAny<long>()))
                                .Returns(Task.FromResult<Vacancy>(null));

            await _sut.Run(vacancyId, userRef, UserEmail, UserName, TransferReason.EmployerRevokedPermission);

            _mockMessaging.Verify(x => x.SendCommandAsync(It.IsAny<TransferVacancyToLegalEntityCommand>()), Times.Never);
        }
    }
}