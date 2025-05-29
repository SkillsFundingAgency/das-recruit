using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Jobs.Communication;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.Testing.AutoFixture;

namespace Recruit.Vacancies.Jobs.UnitTests.Communication;

public class WhenSendingAnEmail
{
    [Test, MoqAutoData]
    public async Task Then_Sends_Correct_Message_To_Notification_Service(
        SendEmailCommand email,
        [Frozen] Mock<IMessageSession> mockMessageSession,
        NotificationService service)
    {
        await service.Send(email);

        mockMessageSession.Verify(session => session.Send(email, It.IsAny<SendOptions>()));
    }
}