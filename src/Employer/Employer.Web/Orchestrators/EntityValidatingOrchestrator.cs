using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public abstract class EntityValidatingOrchestrator<TEntity, TEditModel>
    {
        protected readonly ILogger Logger;
        private EntityToViewModelPropertyMappings<TEntity, TEditModel> _mappings;
        private IDictionary<string, string> _mappingDictionary;

        protected EntityValidatingOrchestrator(ILogger logger)
        {
            Logger = logger;
        }

        private void BuildMappings()
        {
            _mappings = DefineMappings();
            _mappingDictionary = BuildMappingDictionary();
        }
        
        protected abstract EntityToViewModelPropertyMappings<TEntity, TEditModel> DefineMappings();

        protected async Task<OrchestratorResponse> ValidateAndExecute(TEntity entity, Func<TEntity, EntityValidationResult> validationFunc, Func<TEntity, Task> action)
        {
            BuildMappings();
            
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
            BuildMappings();
            
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
            }

            return mappings;
        }
    }
}