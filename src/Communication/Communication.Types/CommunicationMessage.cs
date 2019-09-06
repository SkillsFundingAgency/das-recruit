using System;
using System.Collections.Generic;

namespace Communication.Types
{
    public class CommunicationMessage
    {
        public Guid Id { get; set; }
        public string RequestType { get; set; }
        public string ParticipantsResolverName { get; set; }
        public string OriginatingServiceName { get; set; }
        public CommunicationUser Recipient { get; set; }
        public DeliveryChannel Channel { get; set; }
        public DeliveryFrequency Frequency { get; set; }
        public List<CommunicationDataItem> DataItems { get; set; }
        public string TemplateId { get; set; }
        public DateTime RequestDateTime { get; set; }
        public CommunicationMessageStatus Status { get; set; }
        public DispatchOutcome? DispatchOutcome { get; set; }
        public DateTime? DispatchDateTime { get; set; }
        public Guid? AggregatedMessageId { get; set; }
    }
}
