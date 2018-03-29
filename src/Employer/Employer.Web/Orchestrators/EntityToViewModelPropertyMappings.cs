using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    // This class represents the mappings of the properties in an Entity to those on a particular ViewModel
    public class EntityToViewModelPropertyMappings<TEntity, TViewModel> : List<Tuple<Expression<Func<TEntity, object>>, Expression<Func<TViewModel, object>>>>
    {
        public void Add(Expression<Func<TEntity, object>> source, Expression<Func<TViewModel, object>> destination)
        {
            this.Add(new Tuple<Expression<Func<TEntity, object>>, Expression<Func<TViewModel, object>>>(source, destination));
        }
    }
}