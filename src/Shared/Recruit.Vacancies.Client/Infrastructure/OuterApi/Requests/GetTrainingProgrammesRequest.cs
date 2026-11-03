namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetTrainingProgrammesRequest(int? ukprn = null) : IGetApiRequest
{
    public string GetUrl
    {
        get
        {
            var query = $"trainingprogrammes";

            if (ukprn.HasValue)
            {
                query += $"?ukprn={ukprn.Value}";
            }
            return query;
        }
    }
}