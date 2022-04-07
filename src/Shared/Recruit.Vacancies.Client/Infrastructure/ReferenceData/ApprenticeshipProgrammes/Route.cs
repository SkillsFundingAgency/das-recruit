using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes
{
    public class Route : IRoute
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}