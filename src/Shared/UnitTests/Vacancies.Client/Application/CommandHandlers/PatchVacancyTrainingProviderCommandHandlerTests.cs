using FluentAssertions;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using System.Threading.Tasks;
using System.Threading;
using System;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    [Trait("Category", "Unit")]
    public class PatchVacancyTrainingProviderCommandHandlerTests
    {
        private const long EsfaUkprn = 10033670;
        private const long NonEsfaUkprn = 12345678;
        private readonly Mock<IVacancyRepository> _mockRepository;
        private readonly Mock<ITrainingProviderService> _mockTrainingProvider;
        private readonly PatchVacancyTrainingProviderCommandHandler _handler;
        private Vacancy _updatedVacancy = null;

        public PatchVacancyTrainingProviderCommandHandlerTests()
        {
            _mockRepository = new Mock<IVacancyRepository>();
            _mockTrainingProvider = new Mock<ITrainingProviderService>();
            
            _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Vacancy>()))
                            .Callback<Vacancy>(arg => _updatedVacancy = arg)
                            .Returns(Task.CompletedTask);
            _mockTrainingProvider.Setup(x => x.GetProviderAsync(NonEsfaUkprn))
                                .ReturnsAsync(new TrainingProvider() { Ukprn = NonEsfaUkprn, Name = "FakeProvider", Address = new Address() });

            _handler = new PatchVacancyTrainingProviderCommandHandler(
                Mock.Of<ILogger<PatchVacancyTrainingProviderCommandHandler>>(),
                _mockRepository.Object,
                _mockTrainingProvider.Object
            );
        }

        [Fact]
        public async Task GivenEmployerOwnedVacancy_ThenDoNotProcess()
        {
            var existingVacancy = GetTestEmployerOwnedVacancy();

            _mockRepository.Setup(x => x.GetVacancyAsync(existingVacancy.Id))
                            .ReturnsAsync(existingVacancy);

            var command = new PatchVacancyTrainingProviderCommand(existingVacancy.Id);

            await _handler.Handle(command, CancellationToken.None);

            _mockTrainingProvider.Verify(x => x.GetProviderAsync(It.IsAny<long>()), Times.Never);
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Vacancy>()), Times.Never);
        }

        [Fact]
        public async Task GivenNonEsfaUkprnProviderOwnedVacancy_ThenCallsProvidersApi()
        {
            var existingVacancy = GetTestProviderOwnedVacancy(NonEsfaUkprn);

            _mockRepository.Setup(x => x.GetVacancyAsync(existingVacancy.Id))
                            .ReturnsAsync(existingVacancy);

            var command = new PatchVacancyTrainingProviderCommand(existingVacancy.Id);

            await _handler.Handle(command, CancellationToken.None);

            _updatedVacancy.TrainingProvider.Name.Should().Be("FakeProvider");
            _updatedVacancy.TrainingProvider.Address.Should().NotBeNull();
            _mockTrainingProvider.Verify(x => x.GetProviderAsync(NonEsfaUkprn), Times.AtMostOnce);
            _mockRepository.Verify(x => x.UpdateAsync(_updatedVacancy), Times.AtMostOnce);
        }

        [Fact]
        public async Task GivenEsfaUkprnProviderOwnedVacancy_ThenDoNotCallProvidersApi()
        {
            var existingVacancy = GetTestProviderOwnedVacancy(EsfaUkprn);

            _mockRepository.Setup(x => x.GetVacancyAsync(existingVacancy.Id))
                            .ReturnsAsync(existingVacancy);

            var command = new PatchVacancyTrainingProviderCommand(existingVacancy.Id);

            await _handler.Handle(command, CancellationToken.None);

            _updatedVacancy.TrainingProvider.Name.Should().Be("To be confirmed");
            _updatedVacancy.TrainingProvider.Address.Should().NotBeNull();
            _mockTrainingProvider.Verify(x => x.GetProviderAsync(EsfaUkprn), Times.Never);
            _mockRepository.Verify(x => x.UpdateAsync(_updatedVacancy), Times.AtMostOnce);
        }

        private Vacancy GetTestProviderOwnedVacancy(long ukprn)
        {
            var vacancy = new Vacancy();
            vacancy.Id = Guid.NewGuid();
            vacancy.OwnerType = OwnerType.Provider;
            vacancy.TrainingProvider = new TrainingProvider { Ukprn = ukprn };
            return vacancy;
        }

        private Vacancy GetTestEmployerOwnedVacancy()
        {
            var vacancy = new Vacancy();
            vacancy.Id = Guid.NewGuid();
            vacancy.OwnerType = OwnerType.Employer;
            return vacancy;
        }
    }
}