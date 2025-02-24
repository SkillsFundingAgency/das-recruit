using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Services
{
    public class LegalEntityAgreementServiceTests
    {
        const string EmployerAccountId = "ABCDEF";
        const string AccountLegalEntityPublicHashedId = "ABCDEF";

        private Mock<IEmployerVacancyClient> _clientMock;

        [Test]
        public void HasLegalEntityAgreementAsync_ShouldReturnFalseIfNoMatchingLegalEntity()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, true, "5678",true, "5678");

            var result = sut.HasLegalEntityAgreementAsync(EmployerAccountId, AccountLegalEntityPublicHashedId).Result;

            result.Should().BeFalse();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Never);
        }

        [Test]
        public void HasLegalEntityAgreementAsync_ShouldNotCheckEmployerServiceWhenHasAgreement()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, true, AccountLegalEntityPublicHashedId,true, AccountLegalEntityPublicHashedId);

            var result = sut.HasLegalEntityAgreementAsync(EmployerAccountId, AccountLegalEntityPublicHashedId).Result;

            result.Should().BeTrue();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Never);
        }

        [Test]
        public void HasLegalEntityAgreementAsync_ShouldCheckEmployerServiceWhenHasNoAgreement()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, false, AccountLegalEntityPublicHashedId,true, AccountLegalEntityPublicHashedId);

            var result = sut.HasLegalEntityAgreementAsync(EmployerAccountId, AccountLegalEntityPublicHashedId).Result;

            result.Should().BeTrue();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Once);
            _clientMock.Verify(c => c.SetupEmployerAsync(EmployerAccountId), Times.Once);
        }

        [Test]
        public void HasLegalEntityAgreementAsync_ShouldReturnFalseWhenEmployerServiceLegalEntityHasNoAgreement()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, false, AccountLegalEntityPublicHashedId,false, AccountLegalEntityPublicHashedId);

            var result = sut.HasLegalEntityAgreementAsync(EmployerAccountId, AccountLegalEntityPublicHashedId).Result;

            result.Should().BeFalse();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Once);
            _clientMock.Verify(c => c.SetupEmployerAsync(EmployerAccountId), Times.Never);
        }

        [Test]
        public void HasLegalEntityAgreementAsync_ShouldReturnFalseWhenEmployerServiceCantLocateLegalEntity()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, false, "5678", true, AccountLegalEntityPublicHashedId);

            var result = sut.HasLegalEntityAgreementAsync(EmployerAccountId, AccountLegalEntityPublicHashedId).Result;

            result.Should().BeFalse();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Once);
            _clientMock.Verify(c => c.SetupEmployerAsync(EmployerAccountId), Times.Never);
        }

        private LegalEntityAgreementService GetLegalEntityAgreementService(string employerAccountId, 
            bool hasLegalEntityAgreement, string employerServiceAccountLegalEntityPublicHashedId,
            bool employerServiceHasLegalEntityAgreement, string accountLegalEntityPublicHashedId)
        {
            _clientMock = new Mock<IEmployerVacancyClient>();
            _clientMock.Setup(c => c.GetEditVacancyInfoAsync(employerAccountId)).Returns(Task.FromResult(
                new EmployerEditVacancyInfo
                {
                    LegalEntities = new List<LegalEntity>
                    {
                        new LegalEntity{
                            HasLegalEntityAgreement = hasLegalEntityAgreement,
                            AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId
                        }
                    }
                }));

            _clientMock.Setup(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId)).Returns(Task.FromResult(
                new List<LegalEntity>
                {
                    new LegalEntity
                    {
                        HasLegalEntityAgreement = employerServiceHasLegalEntityAgreement,
                        AccountLegalEntityPublicHashedId = employerServiceAccountLegalEntityPublicHashedId
                    }
                }
                .AsEnumerable()));

            return new LegalEntityAgreementService(_clientMock.Object);
        }
    }
}
