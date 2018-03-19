using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public abstract class EntityValidatingOrchestrator<TEntity, TViewModel>
    {
        private readonly EntityToViewModelPropertyMappings<TEntity, TViewModel> _mappings;
        private readonly IDictionary<string, string> _mappingDictionary;

        public EntityValidatingOrchestrator()
        {
            _mappings = DefineMappings();
            
            _mappingDictionary = BuildMappingDictionary();
        }

        public IDictionary<string, string> PropertyMappingLookup => _mappingDictionary;

        protected abstract EntityToViewModelPropertyMappings<TEntity, TViewModel> DefineMappings();

        protected void MapValidationPropertiesToViewModel(EntityValidationResult validationResult)
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