using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData
{
    public interface IApprenticeshipProgrammesUpdateService
    {
        Task UpdateApprenticeshipProgrammesAsync();
        Task UpdateApprenticeshipRouteAsync();
    }
}
