using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Services
{
    public class TrainingProviderAgreementServiceTests
    {
        private Mock<IProviderVacancyClient> _clientMock;
        private Mock<IGetProviderStatusClient> _pasAccountProviderMock;

        private const long Ukprn = 99999999;

        [Fact]
        public async Task HasAgreement_ShouldReturnFalseWhenPasHasNoAgreement()
        {
            var sut = GetService(false);

            var result = await sut.HasAgreementAsync(Ukprn);

            result.Should().BeFalse();
            _pasAccountProviderMock.Verify(c => c.GetProviderStatus(It.IsAny<long>()), Times.Once);
            _pasAccountProviderMock.Verify(c => c.GetProviderStatus(Ukprn), Times.Once);
            _clientMock.Verify(c => c.SetupProviderAsync(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public async Task HasAgreement_ShouldReturnTrueAndSetupProviderWhenPasHasAgreement()
        {
            var sut = GetService(true);

            var result = await sut.HasAgreementAsync(Ukprn);

            result.Should().BeTrue();
            _pasAccountProviderMock.Verify(c => c.GetProviderStatus(It.IsAny<long>()), Times.Once);
            _pasAccountProviderMock.Verify(c => c.GetProviderStatus(Ukprn), Times.Once);
            _clientMock.Verify(c => c.SetupProviderAsync(It.IsAny<long>()), Times.Once);
            _clientMock.Verify(c => c.SetupProviderAsync(Ukprn), Times.Once);
        }

        private ITrainingProviderAgreementService GetService(bool? pasHasAgreement)
        {
            _clientMock = new Mock<IProviderVacancyClient>();
            
            _pasAccountProviderMock = new Mock<IGetProviderStatusClient>();

            if (pasHasAgreement.HasValue)
            {
                _pasAccountProviderMock.Setup(c => c.GetProviderStatus(Ukprn)).ReturnsAsync(new ProviderAccountResponse{CanAccessService =  pasHasAgreement.Value});
            }
            
            var sut = new TrainingProviderAgreementService(_clientMock.Object, _pasAccountProviderMock.Object);

            return sut;
        }
    }
}
