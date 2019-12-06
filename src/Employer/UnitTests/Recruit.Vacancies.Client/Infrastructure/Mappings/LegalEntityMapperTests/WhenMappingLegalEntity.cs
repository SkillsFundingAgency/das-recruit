using SFA.DAS.EAS.Account.Api.Types;
using Xunit;
using AutoFixture;
using FluentAssertions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using System;
using System.Collections.Generic;

namespace UnitTests.Recruit.Vacancies.Client.Infrastructure.Mappings.LegalEntityMapperTests
{
    public class WhenMappingLegalEntity
    {
        [Fact]
        public void ThenReturnsCorrectlyPolulatedLegalEntity()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture
                .Build<LegalEntityViewModel>()
                .With(x => x.Address, "Cheylesmore House, 5 Quinton Rd, Coventry, CV1 2WT")
                .With( x => x.Agreements, new List<AgreementViewModel>())
                .Create();

            // Act
            var actual = LegalEntityMapper.MapFromAccountApiLegalEntity(expected);

            // Assert
            actual.LegalEntityId.Should().Be(expected.LegalEntityId);
            actual.AccountLegalEntityPublicHashedId.Should().Be(expected.AccountLegalEntityPublicHashedId);
            actual.Name.Should().Be(expected.Name);
            actual.HasLegalEntityAgreement.Should().BeFalse();
        }

        [Fact]
        public void ThenSetsHasLegalEntityAgreementToTrue()
        {
            // Arrange
            var fixture = new Fixture();
            var agreementViewModel = fixture
                .Build<AgreementViewModel>()
                .With(x => x.Status, EmployerAgreementStatus.Signed)
                .Create();
            var expected = fixture
                .Build<LegalEntityViewModel>()
                .With(x => x.Address, "Cheylesmore House, 5 Quinton Rd, Coventry, CV1 2WT")
                .With( x => x.Agreements, new List<AgreementViewModel>{ agreementViewModel }) 
                .Create();

            // Act
            var actual = LegalEntityMapper.MapFromAccountApiLegalEntity(expected);

            // Assert
            actual.HasLegalEntityAgreement.Should().BeTrue();
        }
    }
}