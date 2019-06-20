using System.Linq;
using Communication.Core;
using FluentAssertions;
using Xunit;

namespace Communication.UnitTests.CommunicationProcessorTests.CreateMessagesTest
{
    public class GetOptedInParticipantsTests
    {        
        [Fact]
        public void ShouldReturnParticipantsWhoHaveOptedIn()
        {
            var list = new[] { ParticipantsData.OptedIn, ParticipantsData.OptedOut };

            var result = CommunicationProcessor.GetOptedInParticipants(list).ToList();

            result.Count.Should().Be(1);
            result.Any(p => p.User.Name == ParticipantsData.OptedIn.User.Name).Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnSecondaryParticipantsWithOrganisationalScope()
        {
            var list = new[] { ParticipantsData.PrimaryWithIndividualScope, ParticipantsData.OptedOut, ParticipantsData.SecondaryWithIndividualScope, ParticipantsData.SecondaryWithOrganisationScope };

            var result = CommunicationProcessor.GetOptedInParticipants(list).ToList();

            result.Count.Should().Be(2);
            result.Any(p => p.User.Name == ParticipantsData.PrimaryWithIndividualScope.User.Name).Should().BeTrue();
            result.Any(p => p.User.Name == ParticipantsData.SecondaryWithOrganisationScope.User.Name).Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnPrimaryParticipantsWithAnyScope()
        {
            var list = new[] { ParticipantsData.PrimaryWithIndividualScope, ParticipantsData.PrimaryWithOrganisationalScope };

            var result = CommunicationProcessor.GetOptedInParticipants(list).ToList();

            result.Count.Should().Be(2);
            result.Any(p => p.User.Name == ParticipantsData.PrimaryWithIndividualScope.User.Name).Should().BeTrue();
            result.Any(p => p.User.Name == ParticipantsData.PrimaryWithOrganisationalScope.User.Name).Should().BeTrue();
        }

        [Fact]
        public void ShouldFilterOutPrimaryOptedOut()
        {
            var list = new[] { ParticipantsData.PrimaryOptedOut, ParticipantsData.SecondaryOptedOut };

            var result = CommunicationProcessor.GetOptedInParticipants(list).ToList();

            result.Count.Should().Be(0);
        }
    }
}