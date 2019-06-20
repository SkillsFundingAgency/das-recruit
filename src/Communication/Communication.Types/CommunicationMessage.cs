using System;
using System.Collections.Generic;

namespace Communication.Types
{
    public sealed class CommunicationMessage
    {
        public Guid Id { get; }
        public Guid RequestId { get; internal set; }
        public string RequestType { get; internal set; }
        public string RecipientsResolver { get; internal set; }
        public string OriginatingService { get; internal set; }
        public CommunicationUser Recipient { get; internal set; }
        public DeliveryChannel Channel { get; internal set; }
        public DeliveryFrequency Frequency { get; internal set; }
        public IEnumerable<CommunicationDataItem> DataItems { get; internal set; }
        public string TemplateId { get; internal set; }
        public DateTime RequestDateTime { get; internal set; }

        public CommunicationMessage()
        {
            Id = Guid.NewGuid();
        }
    }
}
