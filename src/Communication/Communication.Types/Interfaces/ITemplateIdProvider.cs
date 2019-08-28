using System.Threading.Tasks;

namespace Communication.Types.Interfaces
{
    public interface ITemplateIdProvider
    {
        /// ProviderServiceName will allow communication processor to map a request to its template provider
        /// This has to be unique value across all the possible services
        /// Example value: VacancyServices.Recruit.Employer
        string ProviderServiceName { get; }
        Task<string> GetTemplateId(CommunicationMessage message, bool isAggregate = false);
    }
}