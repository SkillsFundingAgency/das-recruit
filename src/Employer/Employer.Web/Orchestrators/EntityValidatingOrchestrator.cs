using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public abstract class EntityValidatingOrchestrator<TEntity, TEditModel>
    {
        private readonly EntityToViewModelPropertyMappings<TEntity, TEditModel> _mappings;
        private readonly IDictionary<string, string> _mappingDictionary;
        private readonly ILogger _logger;

        public EntityValidatingOrchestrator(ILogger logger)
        {
            _logger = logger;
            _mappings = DefineMappings();
            
            _mappingDictionary = BuildMappingDictionary();
        }

        public IDictionary<string, string> PropertyMappingLookup => _mappingDictionary;

        protected abstract EntityToViewModelPropertyMappings<TEntity, TEditModel> DefineMappings();

        protected async Task<OrchestratorResponse> ValidateAndExecute(TEntity entity, Func<TEntity, EntityValidationResult> validationFunc, Func<TEntity, Task> action)
        {
            var validationResult = validationFunc.Invoke(entity);

            if (validationResult.HasErrors)
            {
                MapValidationPropertiesToViewModel(validationResult);
                return new OrchestratorResponse(validationResult);
            }

            await action.Invoke(entity);

            return new OrchestratorResponse(true);
        }

        protected async Task<OrchestratorResponse<T>>ValidateAndExecute<T>(TEntity entity, Func<TEntity, EntityValidationResult> validationFunc, Func<TEntity, Task<T>> action)
        {
            var validationResult = validationFunc.Invoke(entity);

            if (validationResult.HasErrors)
            {
                MapValidationPropertiesToViewModel(validationResult);
                return new OrchestratorResponse<T>(validationResult);
            }

            T result = await action.Invoke(entity);

            return new OrchestratorResponse<T>(result);
        }

        private void MapValidationPropertiesToViewModel(EntityValidationResult validationResult)
        {
            foreach(var error in validationResult.Errors)
            {
                if (_mappingDictionary.TryGetValue(error.PropertyName, out string replacementProperty))
                {
                    error.PropertyName = replacementProperty;
                }
            }
        }

        private IDictionary<string, string> BuildMappingDictionary()
        {
            if (_mappings == null)
                return new Dictionary<string, string>(0);

            var mappings = new Dictionary<string, string>(_mappings.Count);

            foreach (var item in _mappings)
            {
                var entityProperty = item.Item1.GetPropertyName();
                var viewModelProperty = item.Item2.GetPropertyName();
                mappings.Add(entityProperty, viewModelProperty);
            };

            return mappings;
        }
        
    }
}