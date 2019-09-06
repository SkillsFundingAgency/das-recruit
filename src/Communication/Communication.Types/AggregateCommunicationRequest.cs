using System;

namespace Communication.Types
{
    public class AggregateCommunicationRequest
    {
        public Guid RequestId { get; }
        public DateTime RequestDateTime { get; }
        public DeliveryFrequency Frequency { get; }
        public string RequestType { get; }
        public DateTime FromDateTime { get; }
        public DateTime ToDateTime { get; }

        public AggregateCommunicationRequest(Guid requestId, string requestType, DeliveryFrequency frequency, DateTime requestDateTime, DateTime fromDateTime, DateTime toDateTime) // end time
        {
            RequestId = requestId;
            RequestType = requestType;
            Frequency = frequency;
            RequestDateTime = requestDateTime;
            FromDateTime = fromDateTime;
            ToDateTime = toDateTime;
        }
    }
}
