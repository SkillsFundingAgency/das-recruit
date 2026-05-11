namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public record PageInfo(ushort RequestedPageNumber, ushort RequestedPageSize, uint TotalCount);

public record PagedDataResponse<T>(T Data, PageInfo PageInfo): DataResponse<T>(Data);