using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Application.Communications.ParticipantResolverPlugins;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Communications
{
    public class VacancyParticipantResolverPluginValidateParticipantsTests
    {
        private static Fixture _fixture = new Fixture();
        private Mock<IVacancyRepository> _mockVacancyRepository = new Mock<IVacancyRepository>();
        private Mock<IUserRepository> _mockUserRepository = new Mock<IUserRepository>();
        private Mock<IUserNotificationPreferencesRepository> _mockUserNotificationPreferenceRepository = new Mock<IUserNotificationPreferencesRepository>();
        private readonly VacancyParticipantsResolverPlugin _sut;

        public VacancyParticipantResolverPluginValidateParticipantsTests()
        {
            _sut = new VacancyParticipantsResolverPlugin(
                _mockVacancyRepository.Object, _mockUserRepository.Object, _mockUserNotificationPreferenceRepository.Object, Mock.Of<ILogger<VacancyParticipantsResolverPlugin>>());
        }

        [Fact]
        public async Task GivenNoCommunicationMessages_WhenValidatingParticipant_ShouldReturnEmptyMessageCollection()
        {
            var validMessages = await _sut.ValidateParticipantAsync(Enumerable.Empty<CommunicationMessage>());

            validMessages.Should().HaveCount(0);
        }

        [Fact]
        public async Task GivenDeferredCommunicationMessage_AndRecipientUserIsNoLongerInRecruit_WhenValidatingParticipant_ShouldReturnEmptyMessageCollection()
        {
            var msg = _fixture.Create<CommunicationMessage>();

            var validMessages = await _sut.ValidateParticipantAsync(new [] {msg});

            validMessages.Should().HaveCount(0);
        }

        [Fact]
        public async Task GivenDeferredCommunicationMessage_AndInvalidVacancyReferenceEntityItem_WhenValidatingParticipant_ShouldReturnEmptyMessageCollection()
        {
            _mockUserRepository.Setup(u => u.GetAsync(It.IsAny<string>()))
                                .ReturnsAsync(new User());
            var msg = _fixture.Create<CommunicationMessage>();
            msg.DataItems.Add( new CommunicationDataItem(CommunicationConstants.DataItemKeys.Vacancy.VacancyReference, "abc"));

            var validMessages = await _sut.ValidateParticipantAsync(new [] {msg});

            validMessages.Should().HaveCount(0);
        }

        [Fact]
        public async Task GivenDeferredCommunicationMessage_AndUserIsEmployerRelatedToVacancy_WhenValidatingParticipant_ShouldReturnMessageInCollection()
        {
            const long VacancyReference = 12345678;
            const string EmployerAccountId = "ABC123";

            var vacancy = _fixture
                .Build<Vacancy>()
                .With(v => v.OwnerType, OwnerType.Employer)
                .With(v => v.EmployerAccountId, EmployerAccountId)
                .With(v => v.SubmittedByUser, _fixture.Create<VacancyUser>())
                .Create();

            _mockVacancyRepository.Setup(v => v.GetVacancyAsync(VacancyReference))
                                    .ReturnsAsync(vacancy);
            _mockUserRepository.Setup(u => u.GetAsync(It.IsAny<string>()))
                                .ReturnsAsync(_fixture.Build<User>().With(u => u.EmployerAccountIds, new [] { EmployerAccountId }).Create());
            _mockUserNotificationPreferenceRepository.Setup(unr => unr.GetAsync(It.IsAny<string>()))
                                                    .ReturnsAsync(new UserNotificationPreferences
                                                    {
                                                        NotificationFrequency = NotificationFrequency.Daily,
                                                        NotificationTypes = NotificationTypes.ApplicationSubmitted
                                                    });
            var msg = _fixture.Build<CommunicationMessage>()
                                .With(x => x.Frequency, DeliveryFrequency.Daily)
                                .With(x => x.RequestType, CommunicationConstants.RequestType.ApplicationSubmitted)
                                .Create();
            msg.DataItems.Add( new CommunicationDataItem(CommunicationConstants.DataItemKeys.Vacancy.VacancyReference, VacancyReference.ToString()));

            var validMessages = await _sut.ValidateParticipantAsync(new [] {msg});

            validMessages.Should().HaveCount(1);
        }
    }
}