using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Xunit;
using static Esfa.Recruit.Vacancies.Client.Application.Communications.CommunicationConstants;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Communications;

public class TemplateIdProviderPluginTests
{
    [Theory]
    [InlineData(RequestType.ProviderBlockedProviderNotification, DeliveryFrequency.Immediate, TemplateIds.ProviderBlockedProviderNotification)]
    public async Task ReturnRespectiveTemplateId(string requestType, DeliveryFrequency frequency, string expectedTemplateId)
    {
        var plugin = new TemplateIdProviderPlugin();

        var message = new CommunicationMessage()
        {
            RequestType = requestType,
            Frequency = frequency
        };

        var templateId = await plugin.GetTemplateIdAsync(message);

        templateId.Should().Be(expectedTemplateId);
    }
}