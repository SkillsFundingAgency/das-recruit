using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public record GetCourseProvidersResponse(IEnumerable<ProviderByLarsCodeItem> Providers);

public record ProviderByLarsCodeItem(string Name, int Ukprn);