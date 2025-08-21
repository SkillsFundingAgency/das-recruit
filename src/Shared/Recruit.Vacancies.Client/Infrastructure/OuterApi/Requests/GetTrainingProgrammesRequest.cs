namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests
{
    public class GetTrainingProgrammesRequest : IGetApiRequest
    {
        private readonly int? _ukprn;

        public GetTrainingProgrammesRequest(int? ukprn = null)
        {
            _ukprn = ukprn;
        }

        public string GetUrl
        {
            get
            {
                var query = $"trainingprogrammes";

                if (_ukprn.HasValue)
                {
                    query += $"?ukprn={_ukprn.Value}";
                }
                return query;
            }
        }
    }
}