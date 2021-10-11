using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    public class UserSignedInCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository = new Mock<IUserRepository>();
        private readonly Mock<IUserNotificationPreferencesRepository> _mockUserNotificationPreferencesRepository = new Mock<IUserNotificationPreferencesRepository>();
        private readonly Mock<ITimeProvider> _mockTimeProvider = new Mock<ITimeProvider>();
        private readonly Mock<IRecruitQueueService> _mockQueueService = new Mock<IRecruitQueueService>();
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public async Task GivenEmployerUser_ShouldRaiseUserAccountUpdateMessage()
        {
            var userType= UserType.Employer;
            var userId = _fixture.Create<Guid>().ToString();
            var user = _fixture.Build<User>().With(u => u.UserType, userType).With(u => u.IdamsUserId, userId).Create();
            var preference = _fixture.Build<UserNotificationPreferences>().With(p => p.Id, userId).Create();
            var vacancyUser = _fixture.Build<VacancyUser>().With(v => v.UserId, userId).Create();
            _fixture.Register(() => new UserSignedInCommand(vacancyUser, userType));
            var command = _fixture.Create<UserSignedInCommand>();

            var sut = GetSut(user, preference);
            await sut.Handle(command, new CancellationToken());

            _mockUserRepository.Verify(u => u.UpsertUserAsync(user));
            _mockUserNotificationPreferencesRepository.Verify(u => u.UpsertAsync(preference));
            _mockQueueService.Verify(q => q.AddMessageAsync(It.Is<UpdateEmployerUserAccountQueueMessage>(u => u.IdamsUserId == user.IdamsUserId)));
        }

        [Fact]
        public async Task GivenProviderUser_ShouldNotRaiseUserAccountUpdateMessage()
        {
            var userType= UserType.Provider;
            var userId = _fixture.Create<Guid>().ToString();
            var user = _fixture.Build<User>().With(u => u.UserType, userType).With(u => u.IdamsUserId, userId).Create();
            var preference = _fixture.Build<UserNotificationPreferences>().With(p => p.Id, userId).Create();
            var vacancyUser = _fixture.Build<VacancyUser>().With(v => v.UserId, userId).Create();
            _fixture.Register(() => new UserSignedInCommand(vacancyUser, userType));
            var command = _fixture.Create<UserSignedInCommand>();

            var sut = GetSut(user, preference);
            await sut.Handle(command, new CancellationToken());

            _mockUserRepository.Verify(u => u.UpsertUserAsync(user));
            _mockUserNotificationPreferencesRepository.Verify(u => u.UpsertAsync(preference));
            _mockQueueService.Verify(q => q.AddMessageAsync(It.IsAny<UpdateEmployerUserAccountQueueMessage>()), Times.Never);
        }

        [Fact]
        public async Task GivenProviderUser_ShouldAssignUkprn()
        {
            var userType= UserType.Provider;
            var userId = _fixture.Create<Guid>().ToString();
            var user = _fixture.Build<User>().With(u => u.UserType, userType).Create();
            var preference = _fixture.Build<UserNotificationPreferences>().With(p => p.Id, userId).Create();
            var vacancyUser = _fixture.Create<VacancyUser>();
            _fixture.Register(() => new UserSignedInCommand(vacancyUser, userType));
            var command = _fixture.Create<UserSignedInCommand>();

            var sut = GetSut(user, preference);
            await sut.Handle(command, new CancellationToken());

            _mockUserRepository.Verify(u => u.UpsertUserAsync(It.Is<User>(c=> c.Ukprn == vacancyUser.Ukprn)));
        }

        [Fact]
        public async Task GivenEmployerUser_ShouldAssignUkprn()
        {
            var userType= UserType.Employer;
            var userId = _fixture.Create<Guid>().ToString();
            var user = _fixture.Build<User>().With(u => u.UserType, userType).Without(u => u.Ukprn).Create();
            var preference = _fixture.Build<UserNotificationPreferences>().With(p => p.Id, userId).Create();
            var vacancyUser = _fixture.Create<VacancyUser>();
            _fixture.Register(() => new UserSignedInCommand(vacancyUser, userType));
            var command = _fixture.Create<UserSignedInCommand>();

            var sut = GetSut(user, preference);
            await sut.Handle(command, new CancellationToken());

            _mockUserRepository.Verify(u => u.UpsertUserAsync(It.Is<User>(c=> c.Ukprn == null)));
        }

        private UserSignedInCommandHandler GetSut(User user, UserNotificationPreferences preference)
        {
            _mockUserRepository.Setup(u => u.GetAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockUserNotificationPreferencesRepository.Setup(u => u.GetAsync(It.IsAny<string>())).ReturnsAsync(preference);

            return new UserSignedInCommandHandler (
                Mock.Of<ILogger<UserSignedInCommandHandler>>(),
                _mockUserRepository.Object,
                _mockUserNotificationPreferencesRepository.Object,
                _mockTimeProvider.Object,
                _mockQueueService.Object
            );
        }
    }
}