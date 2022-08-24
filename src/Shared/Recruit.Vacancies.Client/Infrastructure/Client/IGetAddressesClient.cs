using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IGetAddressesClient
    {
        Task<GetAddressesListResponse> GetAddresses(string searchTerm);
    }
}
