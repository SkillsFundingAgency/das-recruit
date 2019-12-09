using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Services
{
    public class LegalEntityAgreementServiceTests
    {
        const string EmployerAccountId = "ABCDEF";
        const long LegalEntityId = 1234;

        private Mock<IEmployerVacancyClient> _clientMock;

        [Fact]
        public async Task HasLegalEntityAgreementAsync_ShouldReturnFalseIfNoMatchingLegalEntity()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, 5678, true, 5678, true);

            var result = await sut.HasLegalEntityAgreementAsync(EmployerAccountId, LegalEntityId);

            result.Should().BeFalse();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Never);
        }

        [Fact]
        public async Task HasLegalEntityAgreementAsync_ShouldNotCheckEmployerServiceWhenHasAgreement()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, LegalEntityId, true, LegalEntityId, true);

            var result = await sut.HasLegalEntityAgreementAsync(EmployerAccountId, LegalEntityId);

            result.Should().BeTrue();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Never);
        }

        [Fact]
        public async Task HasLegalEntityAgreementAsync_ShouldCheckEmployerServiceWhenHasNoAgreement()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, LegalEntityId, false, LegalEntityId, true);

            var result = await sut.HasLegalEntityAgreementAsync(EmployerAccountId, LegalEntityId);

            result.Should().BeTrue();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Once);
            _clientMock.Verify(c => c.SetupEmployerAsync(EmployerAccountId), Times.Once);
        }

        [Fact]
        public async Task HasLegalEntityAgreementAsync_ShouldReturnFalseWhenEmployerServiceLegalEntityHasNoAgreement()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, LegalEntityId, false, LegalEntityId, false);

            var result = await sut.HasLegalEntityAgreementAsync(EmployerAccountId, LegalEntityId);

            result.Should().BeFalse();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Once);
            _clientMock.Verify(c => c.SetupEmployerAsync(EmployerAccountId), Times.Never);
        }

        [Fact]
        public async Task HasLegalEntityAgreementAsync_ShouldReturnFalseWhenEmployerServiceCantLocateLegalEntity()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, LegalEntityId, false, 5678, true);

            var result = await sut.HasLegalEntityAgreementAsync(EmployerAccountId, LegalEntityId);

            result.Should().BeFalse();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Once);
            _clientMock.Verify(c => c.SetupEmployerAsync(EmployerAccountId), Times.Never);
        }

        private LegalEntityAgreementService GetLegalEntityAgreementService(string employerAccountId, long legalEntityId, bool hasLegalEntityAgreement, long employerServiceLegalEntityId, bool employerServiceHasLegalEntityAgreement)
        {
            _clientMock = new Mock<IEmployerVacancyClient>();
            _clientMock.Setup(c => c.GetEditVacancyInfoAsync(employerAccountId)).Returns(Task.FromResult(
                new EmployerEditVacancyInfo
                {
                    LegalEntities = new List<LegalEntity>
                    {
                        new LegalEntity{LegalEntityId = legalEntityId, HasLegalEntityAgreement = hasLegalEntityAgreement}
                    }
                }));

            _clientMock.Setup(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId)).Returns(Task.FromResult(
                new List<LegalEntity>
                {
                    new LegalEntity
                    {
                    LegalEntityId = employerServiceLegalEntityId,
                    HasLegalEntityAgreement = employerServiceHasLegalEntityAgreement
                    }
                }
                .AsEnumerable()));

            return new LegalEntityAgreementService(_clientMock.Object);
        }
    }
}
