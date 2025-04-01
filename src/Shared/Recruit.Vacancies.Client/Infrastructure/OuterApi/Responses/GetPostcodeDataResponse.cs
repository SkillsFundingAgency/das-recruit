using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public record GetPostcodeDataResponse(string Query, PostcodeData Result);