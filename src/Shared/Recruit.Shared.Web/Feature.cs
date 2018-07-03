﻿using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Shared
{
    public class Feature : IFeature
    {
        private readonly IConfiguration _configuration;
        public Feature(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public bool IsFeatureEnabled(string feature)
        {
            var featureValue = _configuration[$"Features:{feature}"];
            if (string.IsNullOrWhiteSpace(featureValue))
                return false;

            return bool.Parse(featureValue);
        }
    }
}
