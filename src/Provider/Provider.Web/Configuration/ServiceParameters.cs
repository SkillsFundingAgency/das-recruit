using System;

namespace Esfa.Recruit.Provider.Web.Configuration
{
    public class ServiceParameters
    {
        public ServiceParameters(string serviceType)
        {
            VacancyType = Enum.TryParse<VacancyType>(serviceType, true, out var type) 
                ? type 
                : Configuration.VacancyType.Apprenticeship;
        }

        public VacancyType? VacancyType { get; set; }
    }

    public enum VacancyType
    {
        Apprenticeship,
        Traineeship
    }
}