using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Location
{
    public class LegalEntityViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; internal set; }
    }
}