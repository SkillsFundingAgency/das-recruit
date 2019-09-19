using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.PasAccount;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class PasAccountProviderTests
    {
        [Fact]
        public async Task HasAgreementAsync_EsfaTestTrainingProviderShouldHaveAgreement()
        {
            var config = new Mock<IOptions<PasAccountApiConfiguration>>();
            var sut = new PasAccountProvider(config.Object);

            var result = await sut.HasAgreementAsync(EsfaTestTrainingProvider.Ukprn);

            result.Should().BeTrue();
        }
    }
}
