using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public abstract class EntityValidatingOrchestrator<TEntity, TViewModel>
    {
        private readonly EntityToViewModelPropertyMappings<TEntity, TViewModel> _mappings;
        private readonly IDictionary<string, string> _mappingDictionary;
        private readonly ILogger _logger;

        public EntityValidatingOrchestrator(ILogger logger)
        {
            _logger = logger;

            _mappings = DefineMappings();
            
            _mappingDictionary = BuildMappingDictionary();
        }

        public IDictionary<string, string> PropertyMappingLookup => _mappingDictionary;

        protected abstract EntityToViewModelPropertyMappings<TEntity, TViewModel> DefineMappings();

        protected async Task<OrchestratorResponse> BuildOrchestratorResponse(Func<Task> func)
        {
            try
            {
                await func.Invoke();
            }
            catch (EntityValidationException ex)
            {
                _logger.LogDebug("Vacancy update failed validation: {ValidationErrors}", ex.ValidationResult);

                MapValidationPropertiesToViewModel(ex.ValidationResult);
                
                return new OrchestratorResponse(ex.ValidationResult);
            }

            return new OrchestratorResponse(true);
        }

        protected async Task<OrchestratorResponse<T>> BuildOrchestratorResponse<T>(Func<Task<T>> func)
        {
            T data;

            try
            {
                data = await func.Invoke();
            }
            catch (EntityValidationException ex)
            {
                _logger.LogDebug("Vacancy update failed validation: {ValidationErrors}", ex.ValidationResult);

                MapValidationPropertiesToViewModel(ex.ValidationResult);
                
                return new OrchestratorResponse<T>(ex.ValidationResult);
            }

            return new OrchestratorResponse<T>(data);
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
                var entityProperty = GetPropertyName(item.Item1);
                var viewModelProperty = GetPropertyName(item.Item2);
                mappings.Add(entityProperty, viewModelProperty);
            };

            return mappings;
        }

        private string GetPropertyName<T>(Expression<Func<T, object>> propertyExpression)
        {
            SortedSet<string> propertyNames = new SortedSet<string>();

            var getMemberExp = new Func<Expression, MemberExpression>(toUnwrap =>
            {
                if (toUnwrap is UnaryExpression)
                {
                    return ((UnaryExpression)toUnwrap).Operand as MemberExpression;
                }

                return toUnwrap as MemberExpression;
            });

            var memberNames = new Stack<string>();

            var memberExp = getMemberExp(propertyExpression.Body);

            while (memberExp != null)
            {
                memberNames.Push(memberExp.Member.Name);
                memberExp = getMemberExp(memberExp.Expression);
            }

            return string.Join(".", memberNames);
        }
    }
}