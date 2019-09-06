using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Communication.Core;
using Communication.Types;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using FluentAssertions;
using Moq;
using Xunit;

namespace Communication.UnitTests.CommunicationProcessorTests
{
    public class AggregateCommunicationProcessorTests
    {
        private const string TestRequestType = CommunicationConstants.RequestType.ApplicationSubmitted;
        private const string TestUserType = "UserType1";
        private const string TestParticipantsResolverName = "VacancyParticipantsResolverName";
        private static Fixture _fixture = new Fixture();
        private readonly Mock<IParticipantResolver> _mockParticipantResolver;
        private readonly Mock<IUserPreferencesProvider> _mockUserPreferenceProvider;
        private readonly Mock<ICompositeDataItemProvider> _mockComposeDataItemProvider;
        private readonly AggregateCommunicationProcessor _sut;

        public AggregateCommunicationProcessorTests()
        {
            _mockParticipantResolver = new Mock<IParticipantResolver>();
            _mockUserPreferenceProvider = new Mock<IUserPreferencesProvider>();
            _mockComposeDataItemProvider = new Mock<ICompositeDataItemProvider>();

            _mockParticipantResolver.SetupGet(x => x.ParticipantResolverName).Returns(TestParticipantsResolverName);
            _mockUserPreferenceProvider.SetupGet(x => x.UserType).Returns(TestUserType);
            _mockComposeDataItemProvider.SetupGet(x => x.ProviderName).Returns(TestRequestType);

            _sut = new AggregateCommunicationProcessor(new [] {_mockParticipantResolver.Object},
                                                    new [] {_mockUserPreferenceProvider.Object},
                                                    new [] {_mockComposeDataItemProvider.Object});
        }

        [Fact]
        public async Task GivenNoMatchingMessageIds_ThenCreateAggregateMessage_ShouldReturnNullAggregateMessage()
        {
            var acr = _fixture.Create<AggregateCommunicationRequest>();
            var messages = Enumerable.Empty<CommunicationMessage>();
            var aggregateMessage = await _sut.CreateAggregateMessageAsync(acr, messages);

            aggregateMessage.Should().BeNull();
        }

        [Fact]
        public async Task GivenMatchingMessageIds_ThenCreateAggregateMessage_ShouldReturnAggregateMessageWithImmediateFrequency()
        {
            var commPref = new CommunicationUserPreference { Frequency = DeliveryFrequency.Daily, Channels = DeliveryChannelPreferences.EmailOnly, Scope = NotificationScope.Individual };
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow;
            var acr = new AggregateCommunicationRequest(Guid.NewGuid(), TestRequestType, DeliveryFrequency.Daily, DateTime.UtcNow, from, to);
            var messages = Enumerable.Repeat<CommunicationMessage>(GetTestTemplateMessage(), 10);
            _mockUserPreferenceProvider.Setup(x => x.GetUserPreferenceAsync(TestRequestType, It.IsAny<CommunicationUser>()))
                                        .ReturnsAsync(commPref);
            _mockParticipantResolver.Setup(x => x.ValidateParticipantAsync(messages))
                                    .ReturnsAsync(messages);
            var aggregateMessage = await _sut.CreateAggregateMessageAsync(acr, messages);

            aggregateMessage.Should().NotBeNull();
            aggregateMessage.Frequency.Should().Be(DeliveryFrequency.Immediate);
        }

        [Fact]
        public async Task GivenMatchingMessageIds_ThenCreateAggregateMessage_ShouldReturnAggregateMessageWithCorrectProperties()
        {
            var commPref = new CommunicationUserPreference { Frequency = DeliveryFrequency.Daily, Channels = DeliveryChannelPreferences.EmailOnly, Scope = NotificationScope.Individual };
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow;
            var acr = new AggregateCommunicationRequest(Guid.NewGuid(), TestRequestType, DeliveryFrequency.Daily, DateTime.UtcNow, from, to);
            var messages = Enumerable.Repeat<CommunicationMessage>(GetTestTemplateMessage(), 10);
            _mockUserPreferenceProvider.Setup(x => x.GetUserPreferenceAsync(TestRequestType, It.IsAny<CommunicationUser>()))
                                        .ReturnsAsync(commPref);
            _mockParticipantResolver.Setup(x => x.ValidateParticipantAsync(messages))
                                    .ReturnsAsync(messages);
            var aggregateMessage = await _sut.CreateAggregateMessageAsync(acr, messages);

            aggregateMessage.Id.Should().NotBe(messages.First().Id);
            aggregateMessage.Frequency.Should().Be(DeliveryFrequency.Immediate);
            aggregateMessage.RequestType.Should().Be(TestRequestType);
            aggregateMessage.TemplateId.Should().Be(TestRequestType);
            aggregateMessage.DispatchOutcome.Should().BeNull();
        }

        [Theory]
        [InlineData(DeliveryFrequency.Immediate)]
        [InlineData(DeliveryFrequency.Weekly)]
        public async Task GivenMatchingMessageIds_UserPreferenceIsNotDaily_ThenShouldReturnNullAggregateMessage(DeliveryFrequency userFrequencyPreference)
        {
            var commPref = new CommunicationUserPreference { Frequency = userFrequencyPreference, Channels = DeliveryChannelPreferences.EmailOnly, Scope = NotificationScope.Individual };
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow;
            var acr = new AggregateCommunicationRequest(Guid.NewGuid(), TestRequestType, DeliveryFrequency.Daily, DateTime.UtcNow, from, to);
            var messages = Enumerable.Repeat<CommunicationMessage>(GetTestTemplateMessage(), 10);
            _mockUserPreferenceProvider.Setup(x => x.GetUserPreferenceAsync(TestRequestType, It.IsAny<CommunicationUser>()))
                                        .ReturnsAsync(commPref);
            var aggregateMessage = await _sut.CreateAggregateMessageAsync(acr, messages);

            aggregateMessage.Should().BeNull();
        }

        [Fact]
        public async Task GivenMatchingMessageIds_UserPreferenceChannelIsNotSet_ThenShouldReturnNullAggregateMessage()
        {
            var commPref = new CommunicationUserPreference { Frequency = DeliveryFrequency.Daily, Channels = DeliveryChannelPreferences.None, Scope = NotificationScope.Individual };
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow;
            var acr = new AggregateCommunicationRequest(Guid.NewGuid(), TestRequestType, DeliveryFrequency.Daily, DateTime.UtcNow, from, to);
            var messages = Enumerable.Repeat<CommunicationMessage>(GetTestTemplateMessage(), 10);
            _mockUserPreferenceProvider.Setup(x => x.GetUserPreferenceAsync(TestRequestType, It.IsAny<CommunicationUser>()))
                                        .ReturnsAsync(commPref);
            var aggregateMessage = await _sut.CreateAggregateMessageAsync(acr, messages);

            aggregateMessage.Should().BeNull();
        }

        [Fact]
        public async Task GivenMatchingMessageIds_AndMessageIsNoLongerValidParticipation_ShouldReturnAggregateMessageWithImmediateFrequency()
        {
            var commPref = new CommunicationUserPreference { Frequency = DeliveryFrequency.Daily, Channels = DeliveryChannelPreferences.EmailOnly, Scope = NotificationScope.Individual };
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow;
            var acr = new AggregateCommunicationRequest(Guid.NewGuid(), TestRequestType, DeliveryFrequency.Daily, DateTime.UtcNow, from, to);
            var messages = Enumerable.Repeat<CommunicationMessage>(GetTestTemplateMessage(), 10);
            _mockUserPreferenceProvider.Setup(x => x.GetUserPreferenceAsync(TestRequestType, It.IsAny<CommunicationUser>()))
                                        .ReturnsAsync(commPref);
            _mockParticipantResolver.Setup(x => x.ValidateParticipantAsync(messages))
                                    .ReturnsAsync(Enumerable.Empty<CommunicationMessage>());
            var aggregateMessage = await _sut.CreateAggregateMessageAsync(acr, messages);

            aggregateMessage.Should().BeNull();
        }

        [Fact]
        public async Task GivenMatchingMessageIds_AndOnlySubsetOfMessageAreValidParticipation_ShouldReturnAggregateMessage()
        {
            var commPref = new CommunicationUserPreference { Frequency = DeliveryFrequency.Daily, Channels = DeliveryChannelPreferences.EmailOnly, Scope = NotificationScope.Individual };
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow;
            var acr = new AggregateCommunicationRequest(Guid.NewGuid(), TestRequestType, DeliveryFrequency.Daily, DateTime.UtcNow, from, to);
            var messages = Enumerable.Repeat<CommunicationMessage>(GetTestTemplateMessage(), 10);
            _mockUserPreferenceProvider.Setup(x => x.GetUserPreferenceAsync(TestRequestType, It.IsAny<CommunicationUser>()))
                                        .ReturnsAsync(commPref);
            _mockParticipantResolver.Setup(x => x.ValidateParticipantAsync(messages))
                                    .ReturnsAsync(Enumerable.Repeat<CommunicationMessage>(messages.First(), 3));
            var aggregateMessage = await _sut.CreateAggregateMessageAsync(acr, messages);

            aggregateMessage.Should().NotBeNull();
        }

        private CommunicationMessage GetTestTemplateMessage()
        {
            return new CommunicationMessage
            {
                Id = Guid.NewGuid(),
                RequestType = TestRequestType,
                Recipient = new CommunicationUser("userId", "a@a.com", "a", TestUserType, UserParticipation.PrimaryUser),
                ParticipantsResolverName = TestParticipantsResolverName,
                Channel = DeliveryChannel.Email,
                Frequency = DeliveryFrequency.Daily,
                TemplateId = TestRequestType
            };
        }
    }
}