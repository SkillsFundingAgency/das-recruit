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
            //1) Try to resolve using the geocode from an existing vacancy with the same postcode
            var services = new List<IGeocodeService>
            {
                new ExistingVacancyGeocodeService(_logger, _repository)
            };

            //2) Next try postcodes.io
            if (!string.IsNullOrEmpty(_config.PostcodesIoUrl))
            {
                services.Add(new PostcodesIoGeocodeService(_logger, _config.PostcodesIoUrl));
            }

            //3) Next try PostcodeAnywhere
            if (!string.IsNullOrEmpty(_config.PostcodeAnywhereUrl) &&
                !string.IsNullOrEmpty(_config.PostcodeAnywhereKey))
            {
                services.Add(new PostcodeAnywhereGeocodeService(_logger, _config.PostcodeAnywhereUrl, _config.PostcodeAnywhereKey));
            }

            //4) Finally just resolve the outcode
            if (!string.IsNullOrEmpty(_config.PostcodesIoUrl))
            {
                services.Add(new PostcodesIoOutcodeGeocodeService(_logger, _config.PostcodesIoUrl));
            }

            return new GeocodeService(_logger, services);
        }
    }
}
