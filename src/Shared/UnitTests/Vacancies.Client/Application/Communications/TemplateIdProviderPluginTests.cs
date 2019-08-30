using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using FluentAssertions;
using Xunit;
using static Esfa.Recruit.Vacancies.Client.Application.Communications.CommunicationConstants;

namespace UnitTests.Vacancies.Client.Application.Communications
{
    public class TemplateIdProviderPluginTests
    {
        [Theory]
        [InlineData(RequestType.VacancyRejected, DeliveryFrequency.Default, TemplateIds.VacancyRejected)]
        [InlineData(RequestType.ApplicationSubmitted, DeliveryFrequency.Immediate, TemplateIds.ApplicationSubmittedImmediate)]
        [InlineData(RequestType.ProviderBlockedProviderNotification, DeliveryFrequency.Immediate, TemplateIds.ProviderBlockedProviderNotification)]
        [InlineData(RequestType.ApplicationSubmitted, DeliveryFrequency.Daily, "")]
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
}