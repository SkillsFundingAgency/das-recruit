namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests
{
    public class GetTrainingProvidersRequest : IGetApiRequest
    {
        public string GetUrl => $"providers";
    }
}