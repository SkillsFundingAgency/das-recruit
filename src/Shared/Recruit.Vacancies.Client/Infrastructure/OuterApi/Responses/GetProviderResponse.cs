using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses
{
    public class GetProviderResponse
    {
        public Guid Id { get; set; }
        public long Ukprn { get; set; }
        public string LegalName { get; set; }
        public string TradingName { get; set; }
        public bool IsMainProvider { get; set; }
        public ProviderTypeResponse ProviderType { get; set; }
    }

    public class ProviderTypeResponse
    {
        public short Id { get; set; }
    }
}