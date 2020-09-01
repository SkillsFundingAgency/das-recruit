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
        const string AccountLegalEntityPublicHashedId = "XYZPQR";
        private Mock<IEmployerVacancyClient> _clientMock;

        [Fact]
        public async Task HasLegalEntityAgreementAsync_ShouldReturnFalseIfNoMatchingLegalEntity()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, true, "5678",true, "5678");

            var result = await sut.HasLegalEntityAgreementAsync(EmployerAccountId, AccountLegalEntityPublicHashedId);

            result.Should().BeFalse();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Never);
        }

        [Fact]
        public async Task HasLegalEntityAgreementAsync_ShouldNotCheckEmployerServiceWhenHasAgreement()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, true, AccountLegalEntityPublicHashedId,true, AccountLegalEntityPublicHashedId);

            var result = await sut.HasLegalEntityAgreementAsync(EmployerAccountId, AccountLegalEntityPublicHashedId);

            result.Should().BeTrue();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Never);
        }

        [Fact]
        public async Task HasLegalEntityAgreementAsync_ShouldCheckEmployerServiceWhenHasNoAgreement()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, false, AccountLegalEntityPublicHashedId,true, AccountLegalEntityPublicHashedId);

            var result = await sut.HasLegalEntityAgreementAsync(EmployerAccountId, AccountLegalEntityPublicHashedId);

            result.Should().BeTrue();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Once);
            _clientMock.Verify(c => c.SetupEmployerAsync(EmployerAccountId), Times.Once);
        }

        [Fact]
        public async Task HasLegalEntityAgreementAsync_ShouldReturnFalseWhenEmployerServiceLegalEntityHasNoAgreement()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, false, AccountLegalEntityPublicHashedId,false, AccountLegalEntityPublicHashedId);

            var result = await sut.HasLegalEntityAgreementAsync(EmployerAccountId, AccountLegalEntityPublicHashedId);

            result.Should().BeFalse();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Once);
            _clientMock.Verify(c => c.SetupEmployerAsync(EmployerAccountId), Times.Never);
        }

        [Fact]
        public async Task HasLegalEntityAgreementAsync_ShouldReturnFalseWhenEmployerServiceCantLocateLegalEntity()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, false, "5678",true, AccountLegalEntityPublicHashedId);

            var result = await sut.HasLegalEntityAgreementAsync(EmployerAccountId, AccountLegalEntityPublicHashedId);

            result.Should().BeFalse();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Once);
            _clientMock.Verify(c => c.SetupEmployerAsync(EmployerAccountId), Times.Never);
        }

        private LegalEntityAgreementService GetLegalEntityAgreementService(string employerAccountId, bool hasLegalEntityAgreement,
            string employerServiceAccountLegalEntityPublicHashedId, bool employerServiceHasLegalEntityAgreement, 
            string accountLegalEntityPublicHashedId)
        {
            _clientMock = new Mock<IEmployerVacancyClient>();
            _clientMock.Setup(c => c.GetEditVacancyInfoAsync(employerAccountId)).Returns(Task.FromResult(
                new EmployerEditVacancyInfo
                {
                    LegalEntities = new List<LegalEntity>
                    {
                        new LegalEntity
                        {
                            AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
                            HasLegalEntityAgreement = hasLegalEntityAgreement
                        }
                    }
                }));

            _clientMock.Setup(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId)).Returns(Task.FromResult(
                new List<LegalEntity>
                {
                    new LegalEntity
                    {
                        AccountLegalEntityPublicHashedId = employerServiceAccountLegalEntityPublicHashedId,
                        HasLegalEntityAgreement = employerServiceHasLegalEntityAgreement
                    }
                }
                .AsEnumerable()));

            return new LegalEntityAgreementService(_clientMock.Object);
        }
    }
}
