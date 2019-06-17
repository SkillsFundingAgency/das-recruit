using System;
using System.Text;

namespace Communication.Types.Exceptions
{
    public class TemplateIdNotFoundException : Exception
    {
        public TemplateIdNotFoundException(CommunicationMessage communicationMessage) 
            : base(GetErrorMessage(communicationMessage))
        {}

        private static string GetErrorMessage(CommunicationMessage message)
        {
            var builder = new StringBuilder();
            builder.Append($"Template id was not found for message id {message.RequestId}. ");
            builder.Append($"Provider originating service name: {message.OriginatingServiceName}. ");
            builder.Append($"Request type: {message.RequestType}. ");
            builder.Append($"Channel: {message.Channel}. ");
            builder.Append($"Frequency: {message.Frequency}. ");
            builder.Append($"User type: {message.Recipient?.Participation}");
            return builder.ToString();
        }
    }
}