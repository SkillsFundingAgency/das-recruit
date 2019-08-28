using System.Collections.Generic;
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

namespace UnitTests.Vacancies.Client.Application.Communications
{
    public class ParticipantResolverPluginTests
    {
        private static Fixture _fixture = new Fixture();
        private Mock<IVacancyRepository> _mockVacancyRepository = new Mock<IVacancyRepository>();
        private Mock<IUserRepository> _mockUserRepository = new Mock<IUserRepository>();
        private Mock<IUserNotificationPreferencesRepository> _mockUserNotificationPreferenceRepository = new Mock<IUserNotificationPreferencesRepository>();
        private Mock<ILogger<VacancyParticipantsResolverPlugin>> _mockLogger = new Mock<ILogger<VacancyParticipantsResolverPlugin>>();

        private User GetUser(OwnerType owner) => _fixture.Build<User>().With(u => u.Name, owner.ToString()).Create();

        private User GetPrimaryUser(OwnerType owner) => _fixture.Build<User>().With(u => u.IdamsUserId, PrimaryUserId).Create();

        private string PrimaryUserId = _fixture.Create<string>();

        public ParticipantResolverPluginTests()
        {
            _mockUserRepository.Setup(u => u.GetEmployerUsersAsync(It.IsAny<string>())).ReturnsAsync(new List<User> {GetUser(OwnerType.Employer), GetUser(OwnerType.Employer), GetUser(OwnerType.Employer)});
            _mockUserRepository.Setup(u => u.GetProviderUsersAsync(It.IsAny<long>())).ReturnsAsync(new List<User> {GetUser(OwnerType.Provider), GetPrimaryUser(OwnerType.Provider)});
        }

        [Fact]
        public async Task WhenVacancyOwnedByEmployer_GetEmployerUsers()
        {
            var vacancy = _fixture
                .Build<Vacancy>()
                .With(v => v.OwnerType,  OwnerType.Employer)
                .With(v => v.SubmittedByUser, _fixture.Create<VacancyUser>())
                .Create();

            _mockVacancyRepository.Setup(v => v.GetVacancyAsync(It.IsAny<long>())).ReturnsAsync(vacancy);

            var sut = GetSut();

            var request = new CommunicationRequest(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
            request.AddEntity(CommunicationConstants.EntityTypes.Vacancy, _fixture.Create<long>());

            var participants = await sut.GetParticipantsAsync(request);

            participants.Count().Should().Be(3);
            participants.All(p => p.Name == OwnerType.Employer.ToString());
        }

        [Fact]
        public async Task WhenVacancyOwnedByProvider_GetProviderUsers()
        {
            var vacancy = _fixture
                .Build<Vacancy>()
                .With(v => v.OwnerType, OwnerType.Provider)
                .With(v => v.SubmittedByUser, _fixture.Create<VacancyUser>())
                .Create();

            _mockVacancyRepository.Setup(v => v.GetVacancyAsync(It.IsAny<long>())).ReturnsAsync(vacancy);

            var sut = GetSut();

            var request = new CommunicationRequest(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
            request.AddEntity(CommunicationConstants.EntityTypes.Vacancy, _fixture.Create<long>());

            var participants = await sut.GetParticipantsAsync(request);

            participants.Count().Should().Be(2);
            participants.All(p => p.Name == OwnerType.Provider.ToString());
        }

        [Fact]
        public async Task ShouldReturnVacancyOwnerAsPrimaryUser()
        {
            var user = _fixture.Build<VacancyUser>().With(v => v.UserId, PrimaryUserId).Create();
            var vacancy = _fixture
                .Build<Vacancy>()
                .With(v => v.OwnerType,  OwnerType.Provider)
                .With(v => v.SubmittedByUser, user)
                .Create();

            _mockVacancyRepository.Setup(v => v.GetVacancyAsync(It.IsAny<long>())).ReturnsAsync(vacancy);

            var sut = GetSut();

            var request = new CommunicationRequest(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
            request.AddEntity(CommunicationConstants.EntityTypes.Vacancy, _fixture.Create<long>());

            var participants = await sut.GetParticipantsAsync(request);

            participants.Count().Should().Be(2);
            participants.Single(p => p.UserId == PrimaryUserId).Participation.Should().Be(UserParticipation.PrimaryUser);
            participants.Single(p => p.UserId != PrimaryUserId).Participation.Should().Be(UserParticipation.SecondaryUser);
        }

        private VacancyParticipantsResolverPlugin GetSut()
        {
            return new VacancyParticipantsResolverPlugin(
                _mockVacancyRepository.Object, _mockUserRepository.Object, _mockUserNotificationPreferenceRepository.Object, _mockLogger.Object);
        }
    }
}