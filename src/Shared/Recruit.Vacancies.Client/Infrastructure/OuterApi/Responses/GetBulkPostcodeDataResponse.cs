using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public class GetBulkPostcodeDataResponse: List<GetBulkPostcodeDataItemResponse>;

public record GetBulkPostcodeDataItemResponse(string Query, PostcodeData Result);