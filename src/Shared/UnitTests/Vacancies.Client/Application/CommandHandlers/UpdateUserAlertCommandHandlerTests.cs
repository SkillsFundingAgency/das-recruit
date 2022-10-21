using System;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateUserAlertCommandHandlerTests
    {
        [Fact]
        public async Task ShouldUpdateUserTransferredAlert()
        {
            const string userId = "user id";
            DateTime dismissedOn = DateTime.Parse("2019-07-15T11:16");

            var user = new User
            {
                IdamsUserId = userId,
                TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn = null
            };
            
            var clientMock = new Mock<IRecruitVacancyClient>();
            clientMock.Setup(c => c.GetUsersDetailsAsync(userId))
                .ReturnsAsync(user);

            var userRepositoryMock = new Mock<IUserRepository>();

            var sut = new UpdateUserAlertCommandHandler(clientMock.Object, userRepositoryMock.Object);

            var message = new UpdateUserAlertCommand
            {
                IdamsUserId = userId,
                AlertType = AlertType.TransferredVacanciesEmployerRevokedPermission,
                DismissedOn = dismissedOn
            };

            await sut.Handle(message, new CancellationToken());

            userRepositoryMock.Verify(r => r.UpsertUserAsync(user), Times.Once);

            user.TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn.Should().Be(dismissedOn);
        }
    }
}
