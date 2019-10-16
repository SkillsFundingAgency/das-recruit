using System.Threading.Tasks;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.PasAccount;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Services
{
    public class TrainingProviderAgreementServiceTests
    {
        private Mock<IProviderVacancyClient> _clientMock;
        private Mock<IPasAccountProvider> _pasAccountProviderMock;

        private const long Ukprn = 99999999;

        [Fact]
        public async Task HasAgreement_ShouldReturnFalseIfNoMatchingProvider()
        {
            var sut = GetService(null, null);

            var result = await sut.HasAgreementAsync(Ukprn);

            result.Should().BeFalse();
            _pasAccountProviderMock.Verify(c => c.HasAgreementAsync(It.IsAny<long>()), Times.Never);
            _clientMock.Verify(c => c.SetupProviderAsync(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public async Task HasAgreement_ShouldNotCheckPasWhenHasAgreement()
        {
            var sut = GetService(true, true);

            var result = await sut.HasAgreementAsync(Ukprn);

            result.Should().BeTrue();
            _pasAccountProviderMock.Verify(c => c.HasAgreementAsync(It.IsAny<long>()), Times.Never);
            _clientMock.Verify(c => c.SetupProviderAsync(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public async Task HasAgreement_ShouldReturnFalseWhenPasHasNoAgreement()
        {
            var sut = GetService(false, false);

            var result = await sut.HasAgreementAsync(Ukprn);

            result.Should().BeFalse();
            _pasAccountProviderMock.Verify(c => c.HasAgreementAsync(It.IsAny<long>()), Times.Once);
            _pasAccountProviderMock.Verify(c => c.HasAgreementAsync(Ukprn), Times.Once);
            _clientMock.Verify(c => c.SetupProviderAsync(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public async Task HasAgreement_ShouldReturnTrueAndSetupProviderWhenPasHasAgreement()
        {
            var sut = GetService(false, true);

            var result = await sut.HasAgreementAsync(Ukprn);

            result.Should().BeTrue();
            _pasAccountProviderMock.Verify(c => c.HasAgreementAsync(It.IsAny<long>()), Times.Once);
            _pasAccountProviderMock.Verify(c => c.HasAgreementAsync(Ukprn), Times.Once);
            _clientMock.Verify(c => c.SetupProviderAsync(It.IsAny<long>()), Times.Once);
            _clientMock.Verify(c => c.SetupProviderAsync(Ukprn), Times.Once);
        }

        private ITrainingProviderAgreementService GetService(bool? providerEditVacancyHasAgreement, bool? pasHasAgreement)
        {
            _clientMock = new Mock<IProviderVacancyClient>();

            if (providerEditVacancyHasAgreement.HasValue)
            {
                _clientMock.Setup(c => c.GetProviderEditVacancyInfoAsync(Ukprn))
                    .ReturnsAsync(new ProviderEditVacancyInfo
                    {
                        HasProviderAgreement = providerEditVacancyHasAgreement.Value
                    });
            }
            
            _pasAccountProviderMock = new Mock<IPasAccountProvider>();

            if (pasHasAgreement.HasValue)
            {
                _pasAccountProviderMock.Setup(c => c.HasAgreementAsync(Ukprn)).ReturnsAsync(pasHasAgreement.Value);
            }
            
            var sut = new TrainingProviderAgreementService(_clientMock.Object, _pasAccountProviderMock.Object);

            return sut;
        }
    }
}
