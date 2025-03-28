
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;


namespace UnitTests.Recruit.Vacancies.Client.Infrastructure.Mappings.LegalEntityMapperTests
{
    public class WhenMappingLegalEntity
    {
        [Test]
        public void ThenReturnsCorrectlyPolulatedLegalEntity()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture
                .Build<AccountLegalEntity>()
                .With(x => x.Address, "Cheylesmore House, 5 Quinton Rd, Coventry, CV1 2WT")
                .Create();

            // Act
            var actual = LegalEntityMapper.MapFromAccountApiLegalEntity(expected);

            // Assert
            actual.Should().BeEquivalentTo(expected, options=>options
                .Excluding(c=>c.Address)
                .Excluding(c=>c.HasLegalAgreement)
                .Excluding(c=>c.AccountLegalEntityId)
                .Excluding(c=>c.DasAccountId)
                .Excluding(c=>c.LegalEntityId)
            );
            actual.HasLegalEntityAgreement.Should().Be(expected.HasLegalAgreement);
        }

        [Test]
        public void ThenMapsAddress()
        {
            // Arrange
            var fixture = new Fixture();
            
            var expected = fixture
                .Build<AccountLegalEntity>()
                .With(x => x.Address, "Cheylesmore House, 5 Quinton Rd, Coventry, CV1 2WT")
                .Create();

            // Act
            var actual = LegalEntityMapper.MapFromAccountApiLegalEntity(expected);

            // Assert
            actual.Address.AddressLine1.Should().Be("Cheylesmore House");
            actual.Address.AddressLine2.Should().Be("5 Quinton Rd");
            actual.Address.AddressLine3.Should().Be("Coventry");
            actual.Address.AddressLine4.Should().BeNullOrEmpty();
            actual.Address.Postcode.Should().Be("CV1 2WT");
        }
        
        [Test]
        public void Then_Does_Not_Error_If_No_Address()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture
                .Build<AccountLegalEntity>()
                .Create();
            expected.Address = null;

            // Act
            var actual = LegalEntityMapper.MapFromAccountApiLegalEntity(expected);

            // Assert
            actual.Address.Should().BeEquivalentTo(new Address());
        }
    }
}