using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Shared.Web.Orchestrators
{
    public abstract class EntityValidatingOrchestrator<TEntity, TEditModel>
    {
        protected readonly ILogger Logger;

        protected EntityValidatingOrchestrator(ILogger logger)
        {
            Logger = logger;
        }
        
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

        protected void MapValidationPropertiesToViewModel(EntityValidationResult validationResult)
        {
            var mappingDictionary = BuildMappingDictionary();

            foreach (var error in validationResult.Errors)
            {
                if (mappingDictionary.TryGetValue(error.PropertyName, out string replacementProperty))
                {
                    error.PropertyName = replacementProperty;
                }
            }
        }

        private IDictionary<string, string> BuildMappingDictionary()
        {
            var mappings = DefineMappings();

            if (mappings == null)
                return new Dictionary<string, string>(0);

            var mappingDictionary = new Dictionary<string, string>(mappings.Count);

            foreach (var item in mappings)
            {
                var entityProperty = item.Item1.GetPropertyName();
                var viewModelProperty = item.Item2.GetPropertyName();
                mappingDictionary.Add(entityProperty, viewModelProperty);
            }

            return mappingDictionary;
        }
    }
}