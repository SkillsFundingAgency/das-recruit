using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public class GetProvidersResponse
{
    public IEnumerable<GetProviderResponseItem> Providers { get; set; }
}

public class GetProviderResponseItem
{
    public string Name { get; set; }
    public long Ukprn { get; set; }

    /// <summary>
    /// Gets or sets the Training Provider Profile Type Id.
    /// </summary>
    public int ProviderTypeId { get; set; }

    /// <summary>
    /// Gets or sets the Training Provider Status Type Id.
    /// </summary>
    public int StatusId { get; set; }
    public ProviderAddress Address { get; set; }
}

public class ProviderAddress
{
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public string Address4 { get; set; }
    public string Town { get; set; }
    public string Postcode { get; set; }
}