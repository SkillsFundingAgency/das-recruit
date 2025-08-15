namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests
{
    public class GetTrainingProgrammesRequest : IGetApiRequest
    {
        private readonly bool _includeFoundationApprenticeships;
        private readonly int? _ukprn;

        public GetTrainingProgrammesRequest(bool includeFoundationApprenticeships = false, int? ukprn = null)
        {
            _includeFoundationApprenticeships = includeFoundationApprenticeships;
            _ukprn = ukprn;
        }

        public string GetUrl
        {
            get
            {
                var query = $"trainingprogrammes?includeFoundationApprenticeships={_includeFoundationApprenticeships}";

                if (_ukprn.HasValue)
                {
                    query += $"&ukprn={_ukprn.Value}";
                }
                return query;
            }
        }
    }
}