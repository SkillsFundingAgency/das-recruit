using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Esfa.QA.Core.Extensions
{
    public static class ExpressionExtensions
    {
        public static string GetQualifiedFieldId<P>(this Expression<Func<P>> property)
        {
            return GetFieldIdForProperty(property.Body);
        }
        
        public static string GetQualifiedFieldId<T,P>(this Expression<Func<T, P>> property)
        {
            return GetFieldIdForProperty(property.Body);
        }

        private static string GetFieldIdForProperty(Expression propertyBody)
        {
            var memberExpression = propertyBody as MemberExpression;
            var fieldId = string.Empty;

            while (memberExpression != null && memberExpression.Member.MemberType == MemberTypes.Property)
            {
                fieldId = memberExpression.Member.Name + (fieldId != string.Empty ? "." : string.Empty) + fieldId;
                memberExpression = memberExpression.Expression as MemberExpression;
            }

            return fieldId;
        }
    }
}
