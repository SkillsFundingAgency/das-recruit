using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Recruit.Vacancies.Client.Infrastructure.Mappings
{
    public class WhenMappingAddress
    {
        [Fact]
        public void ShouldMapFullFivePartAddress()
        {
            var inputAddr = "Valtech, 46 Colebrooke Row,Islington,London, N1 8AF";

            var actualAddr = LegalEntityMapper.MapFromAddressLine(inputAddr);

            actualAddr.AddressLine1.Should().Be("Valtech");
            actualAddr.AddressLine2.Should().Be("46 Colebrooke Row");
            actualAddr.AddressLine3.Should().Be("Islington");
            actualAddr.AddressLine4.Should().Be("London");
            actualAddr.Postcode.Should().Be("N1 8AF");
        }

        [Fact]
        public void ShouldMapFullThreePartAddress()
        {
            var inputAddr = "46 Colebrooke Row,London, N1 8AF";

            var actualAddr = LegalEntityMapper.MapFromAddressLine(inputAddr);

            actualAddr.AddressLine1.Should().Be("46 Colebrooke Row");
            actualAddr.AddressLine2.Should().Be("London");
            actualAddr.AddressLine3.Should().BeNull();
            actualAddr.AddressLine4.Should().BeNull();

            actualAddr.Postcode.Should().Be("N1 8AF");
        }

        [Fact]
        public void ShouldMapOnlyPostcodeAddress()
        {
            var inputAddr = "N1 8AF";

            var actualAddr = LegalEntityMapper.MapFromAddressLine(inputAddr);

            actualAddr.AddressLine1.Should().BeNull();
            actualAddr.AddressLine2.Should().BeNull();
            actualAddr.AddressLine3.Should().BeNull();
            actualAddr.AddressLine4.Should().BeNull();
            actualAddr.Postcode.Should().Be("N1 8AF");
        }

        [Fact]
        public void ShouldMapOnlySinglePartNonPostcodeAddress()
        {
            var inputAddr = "46 Colebrooke Row";

            var actualAddr = LegalEntityMapper.MapFromAddressLine(inputAddr);

            actualAddr.AddressLine1.Should().Be("46 Colebrooke Row");
            actualAddr.AddressLine2.Should().BeNull();
            actualAddr.AddressLine3.Should().BeNull();
            actualAddr.AddressLine4.Should().BeNull();
            actualAddr.Postcode.Should().BeNull();
        }
    }
}
