using System;
using System.Collections.Generic;
using System.Text;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public class GeocodeServiceFactory : IGeocodeServiceFactory
    {
        private readonly GeocodeConfiguration _config;
        private readonly IVacancyRepository _repository;
        private readonly ILogger<GeocodeService> _logger;

        public GeocodeServiceFactory(IOptions<GeocodeConfiguration> config, ILogger<GeocodeService> logger, IVacancyRepository repository)
        {
            _config = config.Value;
            _repository = repository;
            _logger = logger;
        }

        public IGeocodeService GetGeocodeService()
        {
            var services = new List<IGeocodeService>
            {
                new ExistingVacancyGeocodeService(_logger, _repository)
            };

            if (!string.IsNullOrEmpty(_config.PostcodesIoUrl))
            {
                services.Add(new PostcodesIoGeocodeService(_logger, _config.PostcodesIoUrl));
            }

            return new GeocodeService(services);
        }
    }
}
