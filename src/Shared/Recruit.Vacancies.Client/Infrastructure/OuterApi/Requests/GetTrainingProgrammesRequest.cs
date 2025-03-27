namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests
{
    public class GetTrainingProgrammesRequest(bool includeFoundationApprenticeships = false) : IGetApiRequest
    {
        public string GetUrl => $"trainingprogrammes?includeFoundationApprenticeships={includeFoundationApprenticeships}";
    }
}