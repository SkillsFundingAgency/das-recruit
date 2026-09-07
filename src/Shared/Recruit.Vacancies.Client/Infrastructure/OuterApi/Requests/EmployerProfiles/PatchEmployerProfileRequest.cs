using Esfa.Recruit.Vacancies.Client.Domain.EmployerProfiles;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.EmployerProfiles;

public record PatchEmployerProfileRequest( EmployerProfile Payload) : IPostApiRequest
{
    public string PostUrl => "employer/profiles";
    public object Data { get; set; } = Payload;
}